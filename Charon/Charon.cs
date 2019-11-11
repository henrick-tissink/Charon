using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;

namespace Charon
{
    public static class Charon
    {
        public static void Ferry(string connectionString)
        {
            var sourceAssembly = Assembly.GetCallingAssembly();
            var sqlRepo = new SqlRepo(connectionString);

            var executedScripts = getExecutedScripts(sqlRepo);
            var resourceScriptNames = resourceScripts(sourceAssembly).ToList();
            var scriptsToExecute = resourceScriptNames.Except(executedScripts).ToList();

            if (scriptsToExecute.Any())
                runNewScripts(sqlRepo, scriptsToExecute, sourceAssembly);
        }

        private static IEnumerable<string> getExecutedScripts(SqlRepo repo)
        {
            return repo.GetConnection(connection => connection.Query<string>(@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ExecutedScripts' AND xtype='U')
    CREATE TABLE dbo.ExecutedScripts (
        ScriptId INT IDENTITY PRIMARY KEY,
        ScriptName VARCHAR(MAX) NOT NULL,
        DateApplied DATETIME NOT NULL
    )

SELECT
       ScriptName
FROM dbo.ExecutedScripts
"));
        }

        private static void runNewScripts(SqlRepo repo, IEnumerable<string> newScripts, Assembly sourceAssembly)
        {
            repo.GetConnectionTransaction((connection,transaction) =>
            {
                foreach (var script in newScripts)
                {
                    connection.Execute(loadFile(script, sourceAssembly), transaction: transaction);
                    connection.Execute(@"
INSERT INTO dbo.ExecutedScripts
VALUES(@ScriptName, GETDATE())", new {ScriptName = script}, transaction);
                }
            });
        }



        private static IEnumerable<string> resourceScripts(Assembly sourceAssembly) => 
            sourceAssembly.GetManifestResourceNames().Where(r => r.Contains("MigrationScripts"));

        private static string loadFile(string file, Assembly sourceAssembly)
        {
            using var stream = sourceAssembly.GetManifestResourceStream(file);
            if (stream == null) return string.Empty;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
