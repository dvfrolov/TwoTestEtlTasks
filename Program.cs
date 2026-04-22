using System;
using System.Data;
using System.Data.SqlClient;

namespace TwoTestEtlTasks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //!NB: Подправить для подключения к целевой базе при необходимости,
            // иначе ничего рабоать не будет!
            var connectionString =
                "Server=localhost;Database=TMPDEV;Trusted_Connection=True;TrustServerCertificate=True;";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                { 
                    connection.Open();

                    Console.WriteLine("Connected to database");

                    RunHierarchyEtl(connection);
                    RunUsersDedupEtl(connection);

                    Console.WriteLine("All tasks was completed successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sorry, but some tasks are failed");
                //Console.WriteLine(ex); // можно раскомментировать и посмотреть детали ошибки
            }
            //!NB: Если не надо смотреть на результаты в консоли блок finally можно убрать
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        /*
        Задача 1: Иерархия (Recursive CTE)
         Источник: stage.SourceHierarchy (структура Parent-Child).
         Назначение: dbo.TargetHierarchy.
         Логика: Сформировать полный путь к узлу в формате Root/Child/GrandChild ...
        */
        private static void RunHierarchyEtl(SqlConnection connection)
        {
            const string sql = @"
;WITH HierarchyCTE AS
(
    SELECT
        Id,
        ParentId,
        Name,
        CAST(Name AS NVARCHAR(1000)) AS FullPath
    FROM stage.SourceHierarchy
    WHERE ParentId IS NULL

    UNION ALL

    SELECT
        c.Id,
        c.ParentId,
        c.Name,
        CAST(p.FullPath + N'/' + c.Name AS NVARCHAR(1000)) AS FullPath
    FROM stage.SourceHierarchy c
    INNER JOIN HierarchyCTE p
        ON c.ParentId = p.Id
)
MERGE dbo.TargetHierarchy AS tgt
USING
(
    SELECT Id, ParentId, Name, FullPath
    FROM HierarchyCTE
) AS src
ON tgt.Id = src.Id
WHEN MATCHED AND
(
       ISNULL(tgt.ParentId, -1) <> ISNULL(src.ParentId, -1)
    OR tgt.Name <> src.Name
    OR tgt.FullPath <> src.FullPath
)
THEN UPDATE SET
    tgt.ParentId = src.ParentId,
    tgt.Name = src.Name,
    tgt.FullPath = src.FullPath
WHEN NOT MATCHED BY TARGET
THEN INSERT (Id, ParentId, Name, FullPath)
     VALUES (src.Id, src.ParentId, src.Name, src.FullPath)
 ;";

            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 120;

                var affected = command.ExecuteNonQuery();
                Console.WriteLine($"HierarchyEtl task is done. Rows affected: {affected}");
            };
        }

        /*
        Задача 2: Удалить дубликаты 
         Источник: stage.SourceUsers (могут быть дубликаты пользователей по Email).
         Назначение: dbo.TargetUsers.
         Логика: Оставить только последнюю версию записи (по полю ModifiedDate) для каждого уникального Email.
        */
        private static void RunUsersDedupEtl(SqlConnection connection)
        {
            const string sql = @"
;WITH RankedUsers AS
(
    SELECT 
        Id, Email, UserName, ModifiedDate,
        ROW_NUMBER() OVER
        (PARTITION BY Email ORDER BY ModifiedDate DESC, Id DESC) AS rn
    FROM stage.SourceUsers
),
LatestUsers AS
(
    SELECT
        Id, Email, UserName, ModifiedDate
    FROM RankedUsers
    WHERE rn = 1
)
MERGE dbo.TargetUsers AS tgt
USING LatestUsers AS src ON tgt.Email = src.Email
WHEN MATCHED AND
(
       tgt.Id <> src.Id
    OR tgt.UserName <> src.UserName
    OR tgt.ModifiedDate <> src.ModifiedDate
)
THEN UPDATE SET
    tgt.Id = src.Id,
    tgt.Email = src.Email,
    tgt.UserName = src.UserName,
    tgt.ModifiedDate = src.ModifiedDate
WHEN NOT MATCHED BY TARGET
THEN INSERT (Id, Email, UserName, ModifiedDate)
     VALUES (src.Id, src.Email, src.UserName, src.ModifiedDate)
;";

            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 120;

                var affected = command.ExecuteNonQuery();
                Console.WriteLine($"UsersDedupEtl task is done. Rows affected: {affected}");
            };
        }
    }
}
