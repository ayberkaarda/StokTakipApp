using System;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

public class DatabaseHelper
{
    private readonly string connectionString;

    public DatabaseHelper()
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(settingsPath))
        {
            throw new FileNotFoundException(
                "Veritabanı yapılandırma dosyası appsettings.json bulunamadı. Dosyanın uygulama dizininde bulunduğundan emin olun.",
                settingsPath);
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
        connectionString = configuration.GetConnectionString("StokTakip");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "appsettings.json dosyasında ConnectionStrings:StokTakip bağlantı dizesi eksik veya boş.");
        }
    }

    // SELECT sorguları için
    public DataTable ExecuteQuery(string query)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlDataAdapter da = new SqlDataAdapter(query, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    // Parametresiz INSERT, UPDATE, DELETE sorguları için
    public void ExecuteNonQuery(string query)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // Parametreli INSERT, UPDATE, DELETE sorguları için
    public void ExecuteNonQuery(string query, Dictionary<string, object> parameters)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand(query, conn);

            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
