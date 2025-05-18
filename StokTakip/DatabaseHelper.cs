using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

public class DatabaseHelper
{
    private string connectionString = "Server=MONSTER\\SQLEXPRESS;Database=StokTakip;Trusted_Connection=True;";

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
