﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace SQLPROYECT_C_
{
    public partial class Form1 : Form
    {
        private SqlConnection sql;
        public Form1()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
        }
        private void InitializeDatabaseConnection()
        {
            string connectionString = @"Server=KEILA\SQLEXPRESS01;Database=SQLPROYECT;Integrated Security=True;";
            sql = new SqlConnection(connectionString);
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "CSV files (.csv)|.csv|XML files (.xml)|.xml|JSON files (.json)|.json" };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string extension = Path.GetExtension(filePath).ToLower();
                switch (extension ?? "")
                {
                    case ".csv":
                        {
                            LoadCsv(filePath);
                            break;
                        }
                    case ".xml":
                        {
                            LoadXml(filePath);
                            break;
                        }
                    case ".json":
                        {
                            LoadJson(filePath);
                            break;
                        }

                    default:
                        {
                            MessageBox.Show("Unsupported file format");
                            break;
                        }
                }
            }
        }

        private void SaveDataToDatabase()
        {
            try
            {
                sql.Open();
                var sqlDataAdapter = new SqlDataAdapter("SELECT * FROM STUDENTS", sql);
                var sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter);
                DataTable dataTable = (DataTable)DGV1.DataSource;
                sqlDataAdapter.Update(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
            finally
            {
                sql.Close();
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveDataToDatabase();
        }
        private void LoadDataFromDatabase()
        {
            try
            {
                sql.Open();
                var sqlDataAdapter = new SqlDataAdapter("SELECT * FROM STUDENTS", sql);
                var dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                DGV1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
            finally
            {
                sql.Close();
            }
        }
        private void btnMost_Click(object sender, EventArgs e)
        {
            LoadDataFromDatabase();
        }
        private void LoadCsv(string filePath)
        {
            try
            {
                var dataTable = new DataTable();
                using (var sr = new StreamReader(filePath))
                {
                    string[] headers = sr.ReadLine().Split(',');
                    foreach (var header in headers)
                        dataTable.Columns.Add(header);
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        var dataRow = dataTable.NewRow();
                        for (int i = 0, loopTo = headers.Length - 1; i <= loopTo; i++)
                            dataRow[i] = rows[i];
                        dataTable.Rows.Add(dataRow);
                    }
                }
                DGV1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading CSV: " + ex.Message);
            }
        }
        private void LoadXml(string filePath)
        {
            try
            {
                var dataSet = new DataSet();
                dataSet.ReadXml(filePath);
                DGV1.DataSource = dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading XML: " + ex.Message);
            }
        }
        private void LoadJson(string filePath)
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                var dataTable = JsonConvert.DeserializeObject<DataTable>(jsonData);
                DGV1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading JSON: " + ex.Message);
            }
        }

        private void SaveToXml(string filePath)
        {
            try
            {
                DataTable dataTable = (DataTable)DGV1.DataSource;
                if (dataTable is null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("No data to save!");
                    return;
                }

                var dataSet = new DataSet();
                dataSet.Tables.Add(dataTable.Copy());
                dataSet.WriteXml(filePath);

                MessageBox.Show("Data saved successfully to XML: " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to XML: " + ex.Message);
            }
        }
        private void SaveToCsv(string filePath)
        {
            try
            {
                DataTable dataTable = (DataTable)DGV1.DataSource;
                if (dataTable is null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("No data to save!");
                    return;
                }

                var csvData = new StringBuilder();

                // Write header row
                foreach (DataColumn column in dataTable.Columns)
                    csvData.Append(column.ColumnName + ",");
                csvData.Remove(csvData.Length - 1, 1); // Remove trailing comma
                csvData.AppendLine();

                // Write data rows
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0, loopTo = row.ItemArray.Length - 1; i <= loopTo; i++)
                    {
                        string cellValue = row.ItemArray[i].ToString();
                        // Escape special characters for CSV (optional)
                        // cellValue = cellValue.Replace(",", "\",");
                        csvData.Append(cellValue + ",");
                    }
                    csvData.Remove(csvData.Length - 1, 1); // Remove trailing comma
                    csvData.AppendLine();
                }

                File.WriteAllText(filePath, csvData.ToString());

                MessageBox.Show("Data saved successfully to CSV: " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to CSV: " + ex.Message);
            }
        }
        private void SaveToJson(string filePath)
        {
            try
            {
                DataTable dataTable = (DataTable)DGV1.DataSource;
                if (dataTable is null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("No data to save!");
                    return;
                }

                var jsonData = new List<Dictionary<string, object>>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var rowData = new Dictionary<string, object>();
                    for (int i = 0, loopTo = row.ItemArray.Length - 1; i <= loopTo; i++)
                        rowData.Add(dataTable.Columns[i].ColumnName, row.ItemArray[i]);
                    jsonData.Add(rowData);
                }

                string jsonString = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(filePath, jsonString);

                MessageBox.Show("Data saved successfully to JSON: " + filePath);
            }
            catch (Exception ex)
            {
            }
        }
        private void btnCsv_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (.csv)|.csv";
            saveFileDialog.Title = "Save data to CSV";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                SaveToCsv(filePath);
            }
        }
        private void btnXml_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (.xml)|.xml";
            saveFileDialog.Title = "Save data to XML";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                SaveToXml(filePath);
            }
        }
        private void btnJson_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (.json)|.json";
            saveFileDialog.Title = "Save data to XML";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                SaveToJson(filePath);
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataTable dataTable = (DataTable)DGV1.DataSource;

            // Create a new row with empty values
            var newRow = dataTable.NewRow();
            for (int i = 0, loopTo = dataTable.Columns.Count - 1; i <= loopTo; i++)
                newRow[i] = DBNull.Value; // Set default values as appropriate

            // Add the new row to the DataTable
            dataTable.Rows.Add(newRow);

            // Update the DataGridView to reflect the changes
            DGV1.Refresh();
        }
    }
}
