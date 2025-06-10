using Genbox.Wikipedia.Enums;
using Genbox.Wikipedia.Objects;
using Npgsql;

namespace Genbox.Wikipedia.Examples;

internal static class Program
{
    private static async Task Main()
    {
        var sql = "SELECT species FROM gbif.species ORDER BY species;";
        var connString = "Host=localhost;Username=postgres;Password=system;Database=stikstof";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        var dataSource = dataSourceBuilder.Build();
        var connection = dataSource.OpenConnection();
        var connection_insert = dataSource.OpenConnection();

        NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

        NpgsqlDataReader reader = cmd.ExecuteReader();



        while (reader.Read())
        {
            string val = reader.GetValue(0).ToString();
            Console.Write("Gezocht naar: {0}\n", reader[0]);
            using WikipediaClient client = new WikipediaClient();

            WikiSearchRequest req = new WikiSearchRequest(val);
            req.Limit = 1; //We would like 5 results
            req.WhatToSearch = WikiWhat.Text; //We would like to search inside the articles

            Console.WriteLine();

            req.WikiLanguage = WikiLanguage.Dutch;

            WikiSearchResponse resp = await client.SearchAsync(req).ConfigureAwait(false);

            Console.WriteLine($"Found {resp.QueryResult.SearchResults.Count} Resultaat:");

            foreach (SearchResult s in resp.QueryResult.SearchResults)
            {
                var result = s.Title.Replace("'","\'");
                Console.WriteLine($"" + result + "");
                sql = "UPDATE gbif.species SET name_nl = '" + result + "' WHERE species = '" + val + "';";
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                NpgsqlCommand command = new NpgsqlCommand(sql, connection_insert);
                command.ExecuteNonQuery();
                //connection.Close();


            }
        }

    }


}