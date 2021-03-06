﻿// Copyright (c) 2001-2020 Aspose Pty Ltd. All Rights Reserved.
//
// This file is part of Aspose.Words. The source code in this file
// is only intended as a supplement to the documentation, and is provided
// "as is", without warranty of any kind, either expressed or implied.
//////////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Aspose.Words.Fields;
using Aspose.Words;
using Aspose.Words.MailMerging;
using Aspose.Words.Settings;
using NUnit.Framework;
#if NET462 || JAVA
using System.Web;
using System.Data.Odbc;
#endif

namespace ApiExamples
{
    [TestFixture]
    public class ExMailMerge : ApiExampleBase
    {
#if NET462 || JAVA
        [Test]
        public void ExecuteArray()
        {
            HttpResponse response = null;

            //ExStart
            //ExFor:MailMerge.Execute(String[], Object[])
            //ExFor:ContentDisposition
            //ExFor:Document.Save(HttpResponse,String,ContentDisposition,SaveOptions)
            //ExSummary:Performs a simple insertion of data into merge fields and sends the document to the browser inline.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.InsertField(" MERGEFIELD FullName ");
            builder.InsertParagraph();
            builder.InsertField(" MERGEFIELD Company ");
            builder.InsertParagraph();
            builder.InsertField(" MERGEFIELD Address ");
            builder.InsertParagraph();
            builder.InsertField(" MERGEFIELD City ");

            // Fill the fields in the document with user data
            doc.MailMerge.Execute(new string[] { "FullName", "Company", "Address", "City" },
                new object[] { "James Bond", "MI5 Headquarters", "Milbank", "London" });

            // Send the document in Word format to the client browser with an option to save to disk or open inside the current browser
            Assert.That(() => doc.Save(response, "Artifacts/MailMerge.ExecuteArray.docx", ContentDisposition.Inline, null),
                Throws.TypeOf<ArgumentNullException>()); //Thrown because HttpResponse is null in the test.

            // The response will need to be closed manually to make sure that no superfluous content is added to the document after saving
            Assert.That(() => response.End(), Throws.TypeOf<NullReferenceException>());
            //ExEnd

            doc = DocumentHelper.SaveOpen(doc);

            TestUtil.MailMergeMatchesArray(new[] { new[] { "James Bond", "MI5 Headquarters", "Milbank", "London" } }, doc, true);
        }

        [Test, Category("SkipMono")]
        public void ExecuteDataReader()
        {
            //ExStart
            //ExFor:MailMerge.Execute(IDataReader)
            //ExSummary:Shows how to run a mail merge using data from a data reader.
            // Create a new document and populate it with merge fields
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);
            builder.Write("Product:\t");
            builder.InsertField(" MERGEFIELD ProductName");
            builder.Write("\nSupplier:\t");
            builder.InsertField(" MERGEFIELD CompanyName");
            builder.Writeln();
            builder.InsertField(" MERGEFIELD QuantityPerUnit");
            builder.Write(" for $");
            builder.InsertField(" MERGEFIELD UnitPrice");

            // Create a connection string which points to the "Northwind" database file in our local file system and open a connection, and set up a query
            string connectionString = @"Driver={Microsoft Access Driver (*.mdb)};Dbq=" + DatabaseDir + "Northwind.mdb";
            string query = @"SELECT Products.ProductName, Suppliers.CompanyName, Products.QuantityPerUnit, {fn ROUND(Products.UnitPrice,2)} as UnitPrice
                                        FROM Products 
                                        INNER JOIN Suppliers 
                                        ON Products.SupplierID = Suppliers.SupplierID";

            using (OdbcConnection connection = new OdbcConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // Create an SQL command that will source data for our mail merge
                // The names of the columns returned by this SELECT statement should correspond to the merge fields we placed above
                OdbcCommand command = connection.CreateCommand();
                command.CommandText = query;

                // This will run the command and store the data in the reader
                OdbcDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);

                // Now we can take the data from the reader and use it in the mail merge
                doc.MailMerge.Execute(reader);
            }

            doc.Save(ArtifactsDir + "MailMerge.ExecuteDataReader.docx");
            //ExEnd

            doc = new Document(ArtifactsDir + "MailMerge.ExecuteDataReader.docx");

            TestUtil.MailMergeMatchesQueryResult(DatabaseDir + "Northwind.mdb", query, doc, true);
        }

        //ExStart
        //ExFor:MailMerge.ExecuteADO(Object)
        //ExSummary:Shows how to run a mail merge with data from an ADO dataset.
        [Test, Category("SkipMono")] //ExSkip
        public void ExecuteADO()
        {
            // Create a document that will be merged
            Document doc = CreateSourceDocADOMailMerge();

            // To work with ADO DataSets, we need to add a reference to the Microsoft ActiveX Data Objects library,
            // which is included in the .NET distribution and stored in "adodb.dll", then create a connection
            ADODB.Connection connection = new ADODB.Connection();

            // Create a connection string which points to the "Northwind" database file in our local file system and open a connection
            string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + DatabaseDir + "Northwind.mdb";
            connection.Open(connectionString);

            // Create a record set
            ADODB.Recordset recordset = new ADODB.Recordset();

            // Populate our DataSrt by running an SQL command on the database we are connected to
            // The names of the columns returned here correspond to the values of the MERGEFIELDS that will accommodate our data
            string command = @"SELECT ProductName, QuantityPerUnit, UnitPrice FROM Products";
            recordset.Open(command, connection);

            // Execute the mail merge and save the document
            doc.MailMerge.ExecuteADO(recordset);
            doc.Save(ArtifactsDir + "MailMerge.ExecuteADO.docx");
            TestUtil.MailMergeMatchesQueryResult(DatabaseDir + "Northwind.mdb", command, doc, true); //ExSkip
        }

        /// <summary>
        /// Create a blank document and populate it with MERGEFIELDS that will accept data when a mail merge is executed.
        /// </summary>
        private static Document CreateSourceDocADOMailMerge()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.Write("Product:\t");
            builder.InsertField(" MERGEFIELD ProductName");
            builder.Writeln();
            builder.InsertField(" MERGEFIELD QuantityPerUnit");
            builder.Write(" for $");
            builder.InsertField(" MERGEFIELD UnitPrice");

            return doc;
        }
        //ExEnd

        //ExStart
        //ExFor:MailMerge.ExecuteWithRegionsADO(Object,String)
        //ExSummary:Shows how to run a mail merge with regions, compiled with data from an ADO dataset.
        [Test, Category("SkipMono")] //ExSkip
        public void ExecuteWithRegionsADO()
        {
            // Create a document that will be merged
            Document doc = CreateSourceDocADOMailMergeWithRegions();

            // To work with ADO DataSets, we need to add a reference to the Microsoft ActiveX Data Objects library,
            // which is included in the .NET distribution and stored in "adodb.dll", then create a connection
            ADODB.Connection connection = new ADODB.Connection();

            // Create a connection string which points to the "Northwind" database file in our local file system and open a connection
            string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + DatabaseDir + "Northwind.mdb";
            connection.Open(connectionString);

            // Create a record set
            ADODB.Recordset recordset = new ADODB.Recordset();

            // Create an SQL query that fetches data with column names that are suitable for our first mail merge region,
            // then populate our record set with the data
            string command = "SELECT FirstName, LastName, City FROM Employees";
            recordset.Open(command, connection);

            // Run a mail merge on just the first region, filling its MERGEFIELDS with data from the ADO record set
            doc.MailMerge.ExecuteWithRegionsADO(recordset, "MergeRegion1");

            // Close the record set and reopen it with data from another SQL query
            recordset.Close();
            command = "SELECT * FROM Customers";
            recordset.Open(command, connection);

            // Run a mail merge on the second region and save the document
            doc.MailMerge.ExecuteWithRegionsADO(recordset, "MergeRegion2");

            doc.Save(ArtifactsDir + "MailMerge.ExecuteWithRegionsADO.docx");
            TestUtil.MailMergeMatchesQueryResultMultiple(DatabaseDir + "Northwind.mdb", new[] { "SELECT FirstName, LastName, City FROM Employees", "SELECT ContactName, Address, City FROM Customers" }, new Document(ArtifactsDir + "MailMerge.ExecuteWithRegionsADO.docx"), false); //ExSkip
        }

        /// <summary>
        /// Create a blank document and use MERGEFIELDS to create two sequential mail merge regions with TableStart/TableEnd tags
        /// </summary>
        private static Document CreateSourceDocADOMailMergeWithRegions()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // First mail merge region
            builder.Writeln("\tEmployees: ");
            builder.InsertField(" MERGEFIELD TableStart:MergeRegion1");
            builder.InsertField(" MERGEFIELD FirstName");
            builder.Write(", ");
            builder.InsertField(" MERGEFIELD LastName");
            builder.Write(", ");
            builder.InsertField(" MERGEFIELD City");
            builder.InsertField(" MERGEFIELD TableEnd:MergeRegion1");
            builder.InsertParagraph();

            // Second mail merge region
            builder.Writeln("\tCustomers: ");
            builder.InsertField(" MERGEFIELD TableStart:MergeRegion2");
            builder.InsertField(" MERGEFIELD ContactName");
            builder.Write(", ");
            builder.InsertField(" MERGEFIELD Address");
            builder.Write(", ");
            builder.InsertField(" MERGEFIELD City");
            builder.InsertField(" MERGEFIELD TableEnd:MergeRegion2");

            return doc;
        }
        //ExEnd
#endif

        [Test]
        public void ExecuteDataTable()
        {
            //ExStart
            //ExFor:Document
            //ExFor:MailMerge
            //ExFor:MailMerge.Execute(DataTable)
            //ExFor:MailMerge.Execute(DataRow)
            //ExFor:Document.MailMerge
            //ExSummary:Executes mail merge from an ADO.NET DataTable.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);
            builder.InsertField(" MERGEFIELD CustomerName ");
            builder.InsertParagraph();
            builder.InsertField(" MERGEFIELD Address ");

            // This example creates a table, but you would normally load table from a database
            DataTable table = new DataTable("Test");
            table.Columns.Add("CustomerName");
            table.Columns.Add("Address");
            table.Rows.Add(new object[] { "Thomas Hardy", "120 Hanover Sq., London" });
            table.Rows.Add(new object[] { "Paolo Accorti", "Via Monte Bianco 34, Torino" });

            // Field values from the table are inserted into the mail merge fields found in the document
            doc.MailMerge.Execute(table);

            doc.Save(ArtifactsDir + "MailMerge.ExecuteDataTable.docx");

            // Create a copy of our document to perform another mail merge
            doc = new Document();
            builder = new DocumentBuilder(doc);
            builder.InsertField(" MERGEFIELD CustomerName ");
            builder.InsertParagraph();
            builder.InsertField(" MERGEFIELD Address ");

            // We can also source values for a mail merge from a single row in the table
            doc.MailMerge.Execute(table.Rows[1]);

            doc.Save(ArtifactsDir + "MailMerge.ExecuteDataTable.OneRow.docx");
            //ExEnd

            TestUtil.MailMergeMatchesDataTable(table, new Document(ArtifactsDir + "MailMerge.ExecuteDataTable.docx"), true);

            DataTable rowAsTable = new DataTable();
            rowAsTable.ImportRow(table.Rows[1]);

            TestUtil.MailMergeMatchesDataTable(rowAsTable, new Document(ArtifactsDir + "MailMerge.ExecuteDataTable.OneRow.docx"), true);
        }

        [Test]
        public void ExecuteDataView()
        {
            //ExStart
            //ExFor:MailMerge.Execute(DataView)
            //ExSummary:Shows how to process a DataTable's data with a DataView before using it in a mail merge.
            // Create a new document and populate it with merge fields
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);
            builder.Write("Congratulations ");
            builder.InsertField(" MERGEFIELD Name");
            builder.Write(" for passing with a grade of ");
            builder.InsertField(" MERGEFIELD Grade");

            // Create a data table that merge data will be sourced from 
            DataTable table = new DataTable("ExamResults");
            table.Columns.Add("Name");
            table.Columns.Add("Grade");
            table.Rows.Add(new object[] { "John Doe", "67" });
            table.Rows.Add(new object[] { "Jane Doe", "81" });
            table.Rows.Add(new object[] { "John Cardholder", "47" });
            table.Rows.Add(new object[] { "Joe Bloggs", "75" });

            // If we execute the mail merge on the table, a page will be created for each row in the order that it appears in the table
            // If we want to sort/filter rows without changing the table, we can use a data view
            DataView view = new DataView(table);
            view.Sort = "Grade DESC";
            view.RowFilter = "Grade >= 50";

            // This mail merge will be executed on a view where the rows are sorted by the "Grade" column
            // and rows where the Grade values are below 50 are filtered out
            doc.MailMerge.Execute(view);

            doc.Save(ArtifactsDir + "MailMerge.ExecuteDataView.docx");
            //ExEnd

            TestUtil.MailMergeMatchesDataTable(view.ToTable(), new Document(ArtifactsDir + "MailMerge.ExecuteDataView.docx"), true);
        }

        //ExStart
        //ExFor:MailMerge.ExecuteWithRegions(DataSet)
        //ExSummary:Shows how to create a nested mail merge with regions with data from a data set with two related tables.
        [Test]
        public void ExecuteWithRegionsNested()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // Create a MERGEFIELD with a value of "TableStart:Customers"
            // Normally, MERGEFIELDs specify the name of the column that they take row data from
            // "TableStart:Customers" however means that we are starting a mail merge region which belongs to a table called "Customers"
            // This will start the outer region and an "TableEnd:Customers" MERGEFIELD will signify its end 
            builder.InsertField(" MERGEFIELD TableStart:Customers");

            // Data from rows of the "CustomerName" column of the "Customers" table will go in this MERGEFIELD
            builder.Write("Orders for ");
            builder.InsertField(" MERGEFIELD CustomerName");
            builder.Write(":");

            // Create column headers for a table which will contain values from the second inner region
            builder.StartTable();
            builder.InsertCell();
            builder.Write("Item");
            builder.InsertCell();
            builder.Write("Quantity");
            builder.EndRow();

            // We have a second data table called "Orders", which has a many-to-one relationship with "Customers",
            // related by a "CustomerID" column
            // We will start this inner mail merge region over which the "Orders" table will preside,
            // which will iterate over the "Orders" table once for each merge of the outer "Customers" region,
            // picking up rows with the same CustomerID value
            builder.InsertCell();
            builder.InsertField(" MERGEFIELD TableStart:Orders");
            builder.InsertField(" MERGEFIELD ItemName");
            builder.InsertCell();
            builder.InsertField(" MERGEFIELD Quantity");

            // End the inner region
            // One stipulation of using regions and tables is that the opening and closing of a mail merge region must
            // only happen over one row of a document's table  
            builder.InsertField(" MERGEFIELD TableEnd:Orders");
            builder.EndTable();

            // End the outer region
            builder.InsertField(" MERGEFIELD TableEnd:Customers");

            DataSet customersAndOrders = CreateDataSet();
            doc.MailMerge.ExecuteWithRegions(customersAndOrders);

            doc.Save(ArtifactsDir + "MailMerge.ExecuteWithRegionsNested.docx");
            TestUtil.MailMergeMatchesDataSet(customersAndOrders, new Document(ArtifactsDir + "MailMerge.ExecuteWithRegionsNested.docx"), false); //ExSkip
        }

        /// <summary>
        /// Generates a data set which has two data tables named "Customers" and "Orders", with a one-to-many relationship on the "CustomerID" column.
        /// </summary>
        private static DataSet CreateDataSet()
        {
            // Create the outer mail merge
            DataTable tableCustomers = new DataTable("Customers");
            tableCustomers.Columns.Add("CustomerID");
            tableCustomers.Columns.Add("CustomerName");
            tableCustomers.Rows.Add(new object[] { 1, "John Doe" });
            tableCustomers.Rows.Add(new object[] { 2, "Jane Doe" });

            // Create the table for the inner merge
            DataTable tableOrders = new DataTable("Orders");
            tableOrders.Columns.Add("CustomerID");
            tableOrders.Columns.Add("ItemName");
            tableOrders.Columns.Add("Quantity");
            tableOrders.Rows.Add(new object[] { 1, "Hawaiian", 2 });
            tableOrders.Rows.Add(new object[] { 2, "Pepperoni", 1 });
            tableOrders.Rows.Add(new object[] { 2, "Chicago", 1 });

            // Add both tables to a data set
            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(tableCustomers);
            dataSet.Tables.Add(tableOrders);

            // The "CustomerID" column, also the primary key of the customers table is the foreign key for the Orders table
            dataSet.Relations.Add(tableCustomers.Columns["CustomerID"], tableOrders.Columns["CustomerID"]);

            return dataSet;
        }
        //ExEnd

        [Test]
        public void ExecuteWithRegionsConcurrent()
        {
            //ExStart
            //ExFor:MailMerge.ExecuteWithRegions(DataTable)
            //ExFor:MailMerge.ExecuteWithRegions(DataView)
            //ExSummary:Shows how to use regions to execute two separate mail merges in one document.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // If we want to perform two consecutive mail merges on one document while taking data from two tables
            // that are related to each other in any way, we can separate the mail merges with regions
            // A mail merge region starts and ends with "TableStart:[RegionName]" and "TableEnd:[RegionName]" MERGEFIELDs
            // These regions are separate for unrelated data, while they can be nested for hierarchical data
            builder.Writeln("\tCities: ");
            builder.InsertField(" MERGEFIELD TableStart:Cities");
            builder.InsertField(" MERGEFIELD Name");
            builder.InsertField(" MERGEFIELD TableEnd:Cities");
            builder.InsertParagraph();

            // Both MERGEFIELDs refer to a same column name, but values for each will come from different data tables
            builder.Writeln("\tFruit: ");
            builder.InsertField(" MERGEFIELD TableStart:Fruit");
            builder.InsertField(" MERGEFIELD Name");
            builder.InsertField(" MERGEFIELD TableEnd:Fruit");

            // Create two data tables that are not linked or related in any way which we still want in the same document
            DataTable tableCities = new DataTable("Cities");
            tableCities.Columns.Add("Name");
            tableCities.Rows.Add(new object[] { "Washington" });
            tableCities.Rows.Add(new object[] { "London" });
            tableCities.Rows.Add(new object[] { "New York" });

            DataTable tableFruit = new DataTable("Fruit");
            tableFruit.Columns.Add("Name");
            tableFruit.Rows.Add(new object[] { "Cherry" });
            tableFruit.Rows.Add(new object[] { "Apple" });
            tableFruit.Rows.Add(new object[] { "Watermelon" });
            tableFruit.Rows.Add(new object[] { "Banana" });

            // We will need to run one mail merge per table
            // This mail merge will populate the MERGEFIELDs in the "Cities" range, while leaving the fields in "Fruit" empty
            doc.MailMerge.ExecuteWithRegions(tableCities);

            // Run a second merge for the "Fruit" table
            // We can use a DataView to sort or filter values of a DataTable before it is merged
            DataView dv = new DataView(tableFruit);
            dv.Sort = "Name ASC";
            doc.MailMerge.ExecuteWithRegions(dv);

            doc.Save(ArtifactsDir + "MailMerge.ExecuteWithRegionsConcurrent.docx");
            //ExEnd

            DataSet dataSet = new DataSet();

            dataSet.Tables.Add(tableCities);
            dataSet.Tables.Add(tableFruit);

            TestUtil.MailMergeMatchesDataSet(dataSet, new Document(ArtifactsDir + "MailMerge.ExecuteWithRegionsConcurrent.docx"), false);
        }

        [Test]
        public void MailMergeRegionInfo()
        {
            //ExStart
            //ExFor:MailMerge.GetFieldNamesForRegion(System.String)
            //ExFor:MailMerge.GetFieldNamesForRegion(System.String,System.Int32)
            //ExFor:MailMerge.GetRegionsByName(System.String)
            //ExFor:MailMerge.RegionEndTag
            //ExFor:MailMerge.RegionStartTag
            //ExSummary:Shows how to create, list and read mail merge regions.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // These tags, which go inside MERGEFIELDs, denote the strings that signify the starts and ends of mail merge regions 
            Assert.AreEqual("TableStart", doc.MailMerge.RegionStartTag);
            Assert.AreEqual("TableEnd", doc.MailMerge.RegionEndTag);

            // By using these tags, we will start and end a "MailMergeRegion1", which will contain MERGEFIELDs for two columns
            builder.InsertField(" MERGEFIELD TableStart:MailMergeRegion1");
            builder.InsertField(" MERGEFIELD Column1");
            builder.Write(", ");
            builder.InsertField(" MERGEFIELD Column2");
            builder.InsertField(" MERGEFIELD TableEnd:MailMergeRegion1");

            // We can keep track of merge regions and their columns by looking at these collections
            IList<MailMergeRegionInfo> regions = doc.MailMerge.GetRegionsByName("MailMergeRegion1");
            Assert.AreEqual(1, regions.Count);
            Assert.AreEqual("MailMergeRegion1", regions[0].Name);

            string[] mergeFieldNames = doc.MailMerge.GetFieldNamesForRegion("MailMergeRegion1");
            Assert.AreEqual("Column1", mergeFieldNames[0]);
            Assert.AreEqual("Column2", mergeFieldNames[1]);

            // Insert a region with the same name as an existing region, which will make it a duplicate
            builder.InsertParagraph(); // A single row/paragraph cannot be shared by multiple regions
            builder.InsertField(" MERGEFIELD TableStart:MailMergeRegion1");
            builder.InsertField(" MERGEFIELD Column3");
            builder.InsertField(" MERGEFIELD TableEnd:MailMergeRegion1");

            // Regions that share the same name are still accounted for and can be accessed by index
            regions = doc.MailMerge.GetRegionsByName("MailMergeRegion1");
            Assert.AreEqual(2, regions.Count);

            mergeFieldNames = doc.MailMerge.GetFieldNamesForRegion("MailMergeRegion1", 1);
            Assert.AreEqual("Column3", mergeFieldNames[0]);
            //ExEnd
        }

        //ExStart
        //ExFor:MailMerge.MergeDuplicateRegions
        //ExSummary:Shows how to work with duplicate mail merge regions.
        [TestCase(true)] //ExSkip
        [TestCase(false)] //ExSkip
        public void MergeDuplicateRegions(bool isMergeDuplicateRegions)
        {
            // Create a document and table that we will merge
            Document doc = CreateSourceDocMergeDuplicateRegions();
            DataTable dataTable = CreateSourceTableMergeDuplicateRegions();

            // If this property is false, the first region will be merged
            // while the MERGEFIELDs of the second one will be left in the pre-merge state
            // To get both regions merged we would have to execute the mail merge twice, on a table of the same name
            // If this is set to true, both regions will be affected by the merge
            doc.MailMerge.MergeDuplicateRegions = isMergeDuplicateRegions;

            doc.MailMerge.ExecuteWithRegions(dataTable);
            doc.Save(ArtifactsDir + "MailMerge.MergeDuplicateRegions.docx");
            TestMergeDuplicateRegions(dataTable, doc, isMergeDuplicateRegions); //ExSkip
        }

        /// <summary>
        /// Return a document that contains two duplicate mail merge regions (sharing the same name in the "TableStart/End" tags).
        /// </summary>
        private static Document CreateSourceDocMergeDuplicateRegions()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.InsertField(" MERGEFIELD TableStart:MergeRegion");
            builder.InsertField(" MERGEFIELD Column1");
            builder.InsertField(" MERGEFIELD TableEnd:MergeRegion");
            builder.InsertParagraph();

            builder.InsertField(" MERGEFIELD TableStart:MergeRegion");
            builder.InsertField(" MERGEFIELD Column2");
            builder.InsertField(" MERGEFIELD TableEnd:MergeRegion");

            return doc;
        }

        /// <summary>
        /// Create a data table with one row and two columns.
        /// </summary>
        private static DataTable CreateSourceTableMergeDuplicateRegions()
        {
            DataTable dataTable = new DataTable("MergeRegion");
            dataTable.Columns.Add("Column1");
            dataTable.Columns.Add("Column2");
            dataTable.Rows.Add(new object[] { "Value 1", "Value 2" });

            return dataTable;
        }
        //ExEnd

        private void TestMergeDuplicateRegions(DataTable dataTable, Document doc, bool isMergeDuplicateRegions)
        {
            if (isMergeDuplicateRegions) 
                TestUtil.MailMergeMatchesDataTable(dataTable, doc, true);
            else
            {
                dataTable.Columns.Remove("Column2");
                TestUtil.MailMergeMatchesDataTable(dataTable, doc, true);
            }
        }
        
        //ExStart
        //ExFor:MailMerge.PreserveUnusedTags
        //ExSummary:Shows how to preserve the appearance of alternative mail merge tags that go unused during a mail merge. 
        [TestCase(false)] //ExSkip
        [TestCase(true)] //ExSkip
        public void PreserveUnusedTags(bool doPreserveUnusedTags)
        {
            // Create a document and table that we will merge
            Document doc = CreateSourceDocWithAlternativeMergeFields();
            DataTable dataTable = CreateSourceTablePreserveUnusedTags();

            // By default, alternative merge tags that cannot receive data because the data source has no columns with their name
            // are converted to and left on display as MERGEFIELDs after the mail merge
            // We can preserve their original appearance setting this attribute to true
            doc.MailMerge.PreserveUnusedTags = doPreserveUnusedTags;
            doc.MailMerge.Execute(dataTable);

            doc.Save(ArtifactsDir + "MailMerge.PreserveUnusedTags.docx");

            Assert.AreEqual(doc.GetText().Contains("{{ Column2 }}"), doPreserveUnusedTags);
            TestUtil.MailMergeMatchesDataTable(dataTable, doc, true); //ExSkip
        }

        /// <summary>
        /// Create a document and add two tags that can accept mail merge data that are not the traditional MERGEFIELDs.
        /// </summary>
        private static Document CreateSourceDocWithAlternativeMergeFields()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.Writeln("{{ Column1 }}");
            builder.Writeln("{{ Column2 }}");

            // Our tags will only register as destinations for mail merge data if we set this to true
            doc.MailMerge.UseNonMergeFields = true;

            return doc;
        }

        /// <summary>
        /// Create a simple data table with one column.
        /// </summary>
        private static DataTable CreateSourceTablePreserveUnusedTags()
        {
            DataTable dataTable = new DataTable("MyTable");
            dataTable.Columns.Add("Column1");
            dataTable.Rows.Add(new object[] { "Value1" });

            return dataTable;
        }
        //ExEnd
        
        //ExStart
        //ExFor:MailMerge.MergeWholeDocument
        //ExSummary:Shows the relationship between mail merges with regions and field updating.
        [TestCase(false)] //ExSkip
        [TestCase(true)] //ExSkip
        public void MergeWholeDocument(bool doMergeWholeDocument)
        {
            // Create a document and data table that will both be merged
            Document doc = CreateSourceDocMergeWholeDocument();
            DataTable dataTable = CreateSourceTableMergeWholeDocument();

            // A regular mail merge will update all fields in the document as part of the procedure,
            // which will happen if this property is set to true
            // Otherwise, a mail merge with regions will only update fields
            // within a mail merge region which matches the name of the DataTable
            doc.MailMerge.MergeWholeDocument = doMergeWholeDocument;
            doc.MailMerge.ExecuteWithRegions(dataTable);

            // If true, all fields in the document will be updated upon merging
            // In this case that property is false, so the first QUOTE field will not be updated and will not show a value,
            // but the second one inside the region designated by the data table name will show the correct value
            doc.Save(ArtifactsDir + "MailMerge.MergeWholeDocument.docx");

            Assert.AreEqual(doMergeWholeDocument, doc.GetText().Contains("This QUOTE field is outside of the \"MyTable\" merge region."));
            TestUtil.MailMergeMatchesDataTable(dataTable, doc, true); //ExSkip
        }

        /// <summary>
        /// Create a document with a QUOTE field outside and one more inside a mail merge region called "MyTable"
        /// </summary>
        private static Document CreateSourceDocMergeWholeDocument()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // Insert QUOTE field outside of any mail merge regions
            FieldQuote field = (FieldQuote)builder.InsertField(FieldType.FieldQuote, true);
            field.Text = "This QUOTE field is outside of the \"MyTable\" merge region.";

            // Start "MyTable" merge region
            builder.InsertParagraph();
            builder.InsertField(" MERGEFIELD TableStart:MyTable");

            // Insert QUOTE field inside "MyTable" merge region
            field = (FieldQuote)builder.InsertField(FieldType.FieldQuote, true);
            field.Text = "This QUOTE field is inside the \"MyTable\" merge region.";
            builder.InsertParagraph();

            // Add a MERGEFIELD for a column in the data table, end the "MyTable" region and return the document
            builder.InsertField(" MERGEFIELD MyColumn");
            builder.InsertField(" MERGEFIELD TableEnd:MyTable");

            return doc;
        }

        /// <summary>
        /// Create a simple data table that will be used in a mail merge.
        /// </summary>
        private static DataTable CreateSourceTableMergeWholeDocument()
        {
            DataTable dataTable = new DataTable("MyTable");
            dataTable.Columns.Add("MyColumn");
            dataTable.Rows.Add(new object[] { "MyValue" });

            return dataTable;
        }
        //ExEnd

        //ExStart
        //ExFor:MailMerge.UseWholeParagraphAsRegion
        //ExSummary:Shows the relationship between mail merge regions and paragraphs.
        [Test] //ExSkip
        public void UseWholeParagraphAsRegion()
        {
            // Create a document with 2 mail merge regions in one paragraph and a table to which can fill one of the regions during a mail merge
            Document doc = CreateSourceDocWithNestedMergeRegions();
            DataTable dataTable = CreateSourceTableDataTableForOneRegion();

            // By default, a paragraph can belong to no more than one mail merge region
            // Our document breaks this rule so executing a mail merge with regions now will cause an exception to be thrown
            Assert.True(doc.MailMerge.UseWholeParagraphAsRegion);
            Assert.Throws<InvalidOperationException>(() => doc.MailMerge.ExecuteWithRegions(dataTable));

            // If we set this variable to false, paragraphs and mail merge regions are independent so we can safely run our mail merge
            doc.MailMerge.UseWholeParagraphAsRegion = false;
            doc.MailMerge.ExecuteWithRegions(dataTable);

            // Our first region is populated, while our second is safely displayed as unused all across one paragraph
            doc.Save(ArtifactsDir + "MailMerge.UseWholeParagraphAsRegion.docx");
            TestUtil.MailMergeMatchesDataTable(dataTable, new Document(ArtifactsDir + "MailMerge.UseWholeParagraphAsRegion.docx"), true); //ExSkip
        }

        /// <summary>
        /// Create a document with two mail merge regions sharing one paragraph.
        /// </summary>
        private static Document CreateSourceDocWithNestedMergeRegions()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.Write("Region 1: ");
            builder.InsertField(" MERGEFIELD TableStart:MyTable");
            builder.InsertField(" MERGEFIELD Column1");
            builder.Write(", ");
            builder.InsertField(" MERGEFIELD Column2");
            builder.InsertField(" MERGEFIELD TableEnd:MyTable");

            builder.Write(", Region 2: ");
            builder.InsertField(" MERGEFIELD TableStart:MyOtherTable");
            builder.InsertField(" MERGEFIELD TableEnd:MyOtherTable");

            return doc;
        }

        /// <summary>
        /// Create a data table that can populate one region during a mail merge.
        /// </summary>
        private static DataTable CreateSourceTableDataTableForOneRegion()
        {
            DataTable dataTable = new DataTable("MyTable");
            dataTable.Columns.Add("Column1");
            dataTable.Columns.Add("Column2");
            dataTable.Rows.Add(new object[] { "Value 1", "Value 2" });

            return dataTable;
        }
        //ExEnd

        [TestCase(false)]
        [TestCase(true)]
        public void TrimWhiteSpaces(bool doTrimWhitespaces)
        {
            //ExStart
            //ExFor:MailMerge.TrimWhitespaces
            //ExSummary:Shows how to trimmed whitespaces from mail merge values.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.InsertField("MERGEFIELD myMergeField", null);

            doc.MailMerge.TrimWhitespaces = doTrimWhitespaces;
            doc.MailMerge.Execute(new[] { "myMergeField" }, new object[] { "\t hello world! " });

            if (doTrimWhitespaces)
                Assert.AreEqual("hello world!\f", doc.GetText());
            else
                Assert.AreEqual("\t hello world! \f", doc.GetText());
            //ExEnd
        }

        [Test]
        public void MailMergeGetFieldNames()
        {
            Document doc = new Document();
            //ExStart
            //ExFor:MailMerge.GetFieldNames
            //ExSummary:Shows how to get names of all merge fields in a document.
            string[] fieldNames = doc.MailMerge.GetFieldNames();
            //ExEnd
        }

        [Test]
        public void DeleteFields()
        {
            Document doc = new Document();
            //ExStart
            //ExFor:MailMerge.DeleteFields
            //ExSummary:Shows how to delete all merge fields from a document without executing mail merge.
            doc.MailMerge.DeleteFields();
            //ExEnd
        }

        [Test]
        public void RemoveContainingFields()
        {
            Document doc = new Document();
            //ExStart
            //ExFor:MailMerge.CleanupOptions
            //ExFor:MailMergeCleanupOptions
            //ExSummary:Shows how to instruct the mail merge engine to remove any containing fields from around a merge field during mail merge.
            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveContainingFields;
            //ExEnd
        }

        [Test]
        public void RemoveUnusedFields()
        {
            Document doc = new Document();
            //ExStart
            //ExFor:MailMerge.CleanupOptions
            //ExFor:MailMergeCleanupOptions
            //ExSummary:Shows how to automatically remove unmerged merge fields during mail merge.
            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedFields;
            //ExEnd
        }

        [Test]
        public void RemoveEmptyParagraphs()
        {
            Document doc = new Document();
            //ExStart
            //ExFor:MailMerge.CleanupOptions
            //ExFor:MailMergeCleanupOptions
            //ExSummary:Shows how to make sure empty paragraphs that result from merging fields with no data are removed from the document.
            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveEmptyParagraphs;
            //ExEnd
        }

        [Ignore("WORDSNET-17733")]
        [TestCase("!", false, "")]
        [TestCase(", ", false, "")]
        [TestCase(" . ", false, "")]
        [TestCase(" :", false, "")]
        [TestCase("  ; ", false, "")]
        [TestCase(" ?  ", false, "")]
        [TestCase("  ¡  ", false, "")]
        [TestCase("  ¿  ", false, "")]
        [TestCase("!", true, "!\f")]
        [TestCase(", ", true, ", \f")]
        [TestCase(" . ", true, " . \f")]
        [TestCase(" :", true, " :\f")]
        [TestCase("  ; ", true, "  ; \f")]
        [TestCase(" ?  ", true, " ?  \f")]
        [TestCase("  ¡  ", true, "  ¡  \f")]
        [TestCase("  ¿  ", true, "  ¿  \f")]
        public void RemoveColonBetweenEmptyMergeFields(string punctuationMark,
            bool isCleanupParagraphsWithPunctuationMarks, string resultText)
        {
            //ExStart
            //ExFor:MailMerge.CleanupParagraphsWithPunctuationMarks
            //ExSummary:Shows how to remove paragraphs with punctuation marks after mail merge operation.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            FieldMergeField mergeFieldOption1 = (FieldMergeField) builder.InsertField("MERGEFIELD", "Option_1");
            mergeFieldOption1.FieldName = "Option_1";

            // Here is the complete list of cleanable punctuation marks: ! , . : ; ? ¡ ¿
            builder.Write(punctuationMark);

            FieldMergeField mergeFieldOption2 = (FieldMergeField) builder.InsertField("MERGEFIELD", "Option_2");
            mergeFieldOption2.FieldName = "Option_2";

            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveEmptyParagraphs;
            // The default value of the option is true which means that the behavior was changed to mimic MS Word
            // We can revert to the old behavior by setting the option to false
            doc.MailMerge.CleanupParagraphsWithPunctuationMarks = isCleanupParagraphsWithPunctuationMarks;

            doc.MailMerge.Execute(new[] { "Option_1", "Option_2" }, new object[] { null, null });

            doc.Save(ArtifactsDir + "MailMerge.RemoveColonBetweenEmptyMergeFields.docx");
            //ExEnd

            Assert.AreEqual(resultText, doc.GetText());
        }

        //ExStart
        //ExFor:MailMerge.MappedDataFields
        //ExFor:MappedDataFieldCollection
        //ExFor:MappedDataFieldCollection.Add
        //ExFor:MappedDataFieldCollection.Clear
        //ExFor:MappedDataFieldCollection.ContainsKey(String)
        //ExFor:MappedDataFieldCollection.ContainsValue(String)
        //ExFor:MappedDataFieldCollection.Count
        //ExFor:MappedDataFieldCollection.GetEnumerator
        //ExFor:MappedDataFieldCollection.Item(String)
        //ExFor:MappedDataFieldCollection.Remove(String)
        //ExSummary:Shows how to map data columns and MERGEFIELDs with different names so the data is transferred between them during a mail merge.
        [Test] //ExSkip
        public void MappedDataFieldCollection()
        {
            // Create a document and table that we will merge
            Document doc = CreateSourceDocMappedDataFields();
            DataTable dataTable = CreateSourceTableMappedDataFields();

            // We have a column "Column2" in the data table that does not have a respective MERGEFIELD in the document
            // Also, we have a MERGEFIELD named "Column3" that does not exist as a column in the data source
            // If data from "Column2" is suitable for the "Column3" MERGEFIELD,
            // we can map that column name to the MERGEFIELD in the "MappedDataFields" key/value pair
            MappedDataFieldCollection mappedDataFields = doc.MailMerge.MappedDataFields;

            // A data source column name is linked to a MERGEFIELD name by adding an element like this
            mappedDataFields.Add("MergeFieldName", "DataSourceColumnName");

            // So, values from "Column2" will now go into MERGEFIELDs named "Column3" as well as "Column2", if there are any
            mappedDataFields.Add("Column3", "Column2");

            // The MERGEFIELD name is the "key" to the respective data source column name "value"
            Assert.AreEqual("DataSourceColumnName", mappedDataFields["MergeFieldName"]);
            Assert.True(mappedDataFields.ContainsKey("MergeFieldName"));
            Assert.True(mappedDataFields.ContainsValue("DataSourceColumnName"));

            // Now if we run this mail merge, the "Column3" MERGEFIELDs will take data from "Column2" of the table
            doc.MailMerge.Execute(dataTable);

            // We can count and iterate over the mapped columns/fields
            Assert.AreEqual(2, mappedDataFields.Count);

            using (IEnumerator<KeyValuePair<string, string>> enumerator = mappedDataFields.GetEnumerator())
                while (enumerator.MoveNext())
                    Console.WriteLine(
                        $"Column named {enumerator.Current.Value} is mapped to MERGEFIELDs named {enumerator.Current.Key}");

            // We can also remove some or all of the elements
            mappedDataFields.Remove("MergeFieldName");
            Assert.False(mappedDataFields.ContainsKey("MergeFieldName"));
            Assert.False(mappedDataFields.ContainsValue("DataSourceColumnName"));

            mappedDataFields.Clear();
            Assert.AreEqual(0, mappedDataFields.Count);

            // Removing the mapped key/value pairs has no effect on the document because the merge was already done with them in place
            doc.Save(ArtifactsDir + "MailMerge.MappedDataFieldCollection.docx");
            TestUtil.MailMergeMatchesDataTable(dataTable, new Document(ArtifactsDir + "MailMerge.MappedDataFieldCollection.docx"), true); //ExSkip
        }

        /// <summary>
        /// Create a document with 2 MERGEFIELDs, one of which does not have a corresponding column in the data table.
        /// </summary>
        private static Document CreateSourceDocMappedDataFields()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // Insert two MERGEFIELDs that will accept data from that table
            builder.InsertField(" MERGEFIELD Column1");
            builder.Write(", ");
            builder.InsertField(" MERGEFIELD Column3");

            return doc;
        }

        /// <summary>
        /// Create a data table with 2 columns, one of which does not have a corresponding MERGEFIELD in our source document.
        /// </summary>
        private static DataTable CreateSourceTableMappedDataFields()
        {
            // Create a data table that will be used in a mail merge
            DataTable dataTable = new DataTable("MyTable");
            dataTable.Columns.Add("Column1");
            dataTable.Columns.Add("Column2");
            dataTable.Rows.Add(new object[] { "Value1", "Value2" });

            return dataTable;
        }
        //ExEnd

        [Test]
        public void GetFieldNames()
        {
            //ExStart
            //ExFor:FieldAddressBlock
            //ExFor:FieldAddressBlock.GetFieldNames
            //ExSummary:Shows how to get mail merge field names used by the field.
            Document doc = new Document(MyDir + "Field sample - ADDRESSBLOCK.docx");

            string[] addressFieldsExpect =
            {
                "Company", "First Name", "Middle Name", "Last Name", "Suffix", "Address 1", "City", "State",
                "Country or Region", "Postal Code"
            };

            FieldAddressBlock addressBlockField = (FieldAddressBlock) doc.Range.Fields[0];
            string[] addressBlockFieldNames = addressBlockField.GetFieldNames();
            //ExEnd

            Assert.AreEqual(addressFieldsExpect, addressBlockFieldNames);

            string[] greetingFieldsExpect = { "Courtesy Title", "Last Name" };

            FieldGreetingLine greetingLineField = (FieldGreetingLine) doc.Range.Fields[1];
            string[] greetingLineFieldNames = greetingLineField.GetFieldNames();

            Assert.AreEqual(greetingFieldsExpect, greetingLineFieldNames);
        }

        [Test]
        public void UseNonMergeFields()
        {
            Document doc = new Document();
            //ExStart
            //ExFor:MailMerge.UseNonMergeFields
            //ExSummary:Shows how to perform mail merge into merge fields and into additional fields types.
            doc.MailMerge.UseNonMergeFields = true;
            //ExEnd
        }

        /// <summary>
        /// Without TestCaseSource/TestCase because of some strange behavior when using long data.
        /// </summary>
        [Test]
        public void MustacheTemplateSyntaxTrue()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);
            builder.Write("{{ testfield1 }}");
            builder.Write("{{ testfield2 }}");
            builder.Write("{{ testfield3 }}");

            doc.MailMerge.UseNonMergeFields = true;
            doc.MailMerge.PreserveUnusedTags = true;

            DataTable table = new DataTable("Test");
            table.Columns.Add("testfield2");
            table.Rows.Add("value 1");

            doc.MailMerge.Execute(table);

            string paraText = DocumentHelper.GetParagraphText(doc, 0);

            Assert.AreEqual("{{ testfield1 }}value 1{{ testfield3 }}\f", paraText);
        }

        [Test]
        public void MustacheTemplateSyntaxFalse()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);
            builder.Write("{{ testfield1 }}");
            builder.Write("{{ testfield2 }}");
            builder.Write("{{ testfield3 }}");

            doc.MailMerge.UseNonMergeFields = true;
            doc.MailMerge.PreserveUnusedTags = false;

            DataTable table = new DataTable("Test");
            table.Columns.Add("testfield2");
            table.Rows.Add("value 1");

            doc.MailMerge.Execute(table);

            string paraText = DocumentHelper.GetParagraphText(doc, 0);

            Assert.AreEqual("\u0013MERGEFIELD \"testfield1\"\u0014«testfield1»\u0015value 1\u0013MERGEFIELD \"testfield3\"\u0014«testfield3»\u0015\f", paraText);
        }

        [Test]
        public void TestMailMergeGetRegionsHierarchy()
        {
            //ExStart
            //ExFor:MailMerge.GetRegionsHierarchy
            //ExFor:MailMergeRegionInfo
            //ExFor:MailMergeRegionInfo.Regions
            //ExFor:MailMergeRegionInfo.Name
            //ExFor:MailMergeRegionInfo.Fields
            //ExFor:MailMergeRegionInfo.StartField
            //ExFor:MailMergeRegionInfo.EndField
            //ExFor:MailMergeRegionInfo.Level
            //ExSummary:Shows how to get MailMergeRegionInfo and work with it.
            Document doc = new Document(MyDir + "Mail merge regions.docx");

            // Returns a full hierarchy of regions (with fields) available in the document
            MailMergeRegionInfo regionInfo = doc.MailMerge.GetRegionsHierarchy();

            // Get top regions in the document
            IList<MailMergeRegionInfo> topRegions = regionInfo.Regions;
            Assert.AreEqual(2, topRegions.Count);
            Assert.AreEqual("Region1", topRegions[0].Name);
            Assert.AreEqual("Region2", topRegions[1].Name);
            Assert.AreEqual(1, topRegions[0].Level);
            Assert.AreEqual(1, topRegions[1].Level);

            // Get nested region in first top region
            IList<MailMergeRegionInfo> nestedRegions = topRegions[0].Regions;
            Assert.AreEqual(2, nestedRegions.Count);
            Assert.AreEqual("NestedRegion1", nestedRegions[0].Name);
            Assert.AreEqual("NestedRegion2", nestedRegions[1].Name);
            Assert.AreEqual(2, nestedRegions[0].Level);
            Assert.AreEqual(2, nestedRegions[1].Level);

            // Get field list in first top region
            IList<Field> fieldList = topRegions[0].Fields;
            Assert.AreEqual(4, fieldList.Count);

            FieldMergeField startFieldMergeField = nestedRegions[0].StartField;
            Assert.AreEqual("TableStart:NestedRegion1", startFieldMergeField.FieldName);

            FieldMergeField endFieldMergeField = nestedRegions[0].EndField;
            Assert.AreEqual("TableEnd:NestedRegion1", endFieldMergeField.FieldName);
            //ExEnd
        }

        [Test]
        public void TestTagsReplacedEventShouldRisedWithUseNonMergeFieldsOption()
        {
            //ExStart
            //ExFor:MailMerge.MailMergeCallback
            //ExFor:IMailMergeCallback
            //ExFor:IMailMergeCallback.TagsReplaced
            //ExSummary:Shows how to define custom logic for handling events during mail merge.
            Document document = new Document();
            document.MailMerge.UseNonMergeFields = true;

            MailMergeCallbackStub mailMergeCallbackStub = new MailMergeCallbackStub();
            document.MailMerge.MailMergeCallback = mailMergeCallbackStub;

            document.MailMerge.Execute(new string[0], new object[0]);

            Assert.AreEqual(1, mailMergeCallbackStub.TagsReplacedCounter);
        }

        private class MailMergeCallbackStub : IMailMergeCallback
        {
            public void TagsReplaced()
            {
                TagsReplacedCounter++;
            }

            public int TagsReplacedCounter { get; private set; }
        }
        //ExEnd

        [Test]
        public void GetRegionsByName()
        {
            Document doc = new Document(MyDir + "Mail merge regions.docx");

            IList<MailMergeRegionInfo> regions = doc.MailMerge.GetRegionsByName("Region1");
            Assert.AreEqual(1, doc.MailMerge.GetRegionsByName("Region1").Count);
            foreach (MailMergeRegionInfo region in regions) Assert.AreEqual("Region1", region.Name);

            regions = doc.MailMerge.GetRegionsByName("Region2");
            Assert.AreEqual(1, doc.MailMerge.GetRegionsByName("Region2").Count);
            foreach (MailMergeRegionInfo region in regions) Assert.AreEqual("Region2", region.Name);

            regions = doc.MailMerge.GetRegionsByName("NestedRegion1");
            Assert.AreEqual(2, doc.MailMerge.GetRegionsByName("NestedRegion1").Count);
            foreach (MailMergeRegionInfo region in regions) Assert.AreEqual("NestedRegion1", region.Name);
        }

        [Test]
        public void CleanupOptions()
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);
            builder.StartTable();
            builder.InsertCell();
            builder.InsertField(" MERGEFIELD  TableStart:StudentCourse ");
            builder.InsertCell();
            builder.InsertField(" MERGEFIELD  CourseName ");
            builder.InsertCell();
            builder.InsertField(" MERGEFIELD  TableEnd:StudentCourse ");
            builder.EndTable();

            DataTable data = GetDataTable();

            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveEmptyTableRows;
            doc.MailMerge.ExecuteWithRegions(data);

            doc.Save(ArtifactsDir + "MailMerge.CleanupOptions.docx");

            Assert.IsTrue(DocumentHelper.CompareDocs(ArtifactsDir + "MailMerge.CleanupOptions.docx", GoldsDir + "MailMerge.CleanupOptions Gold.docx"));
        }

        /// <summary>
        /// Create DataTable and fill it with data.
        /// In real life this DataTable should be filled from a database.
        /// </summary>
        private static DataTable GetDataTable()
        {
            DataTable dataTable = new DataTable("StudentCourse");
            dataTable.Columns.Add("CourseName");

            DataRow dataRowEmpty = dataTable.NewRow();
            dataTable.Rows.Add(dataRowEmpty);
            dataRowEmpty[0] = string.Empty;

            for (int i = 0; i < 10; i++)
            {
                DataRow datarow = dataTable.NewRow();
                dataTable.Rows.Add(datarow);
                datarow[0] = "Course " + i;
            }

            return dataTable;
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UnconditionalMergeFieldsAndRegions(bool doCountAllMergeFields)
        {
            //ExStart
            //ExFor:MailMerge.UnconditionalMergeFieldsAndRegions
            //ExSummary:Shows how to merge fields or regions regardless of the parent IF field's condition.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // Insert a MERGEFIELD nested inside an IF field
            // Since the statement of the IF field is false, the result of the inner MERGEFIELD will not be displayed
            // and the MERGEFIELD will not receive any data during a mail merge
            FieldIf fieldIf = (FieldIf)builder.InsertField(" IF 1 = 2 ");
            builder.MoveTo(fieldIf.Separator);
            builder.InsertField(" MERGEFIELD  FullName ");

            // We can still count MERGEFIELDs inside IF fields with false statements if we set this flag to true
            doc.MailMerge.UnconditionalMergeFieldsAndRegions = doCountAllMergeFields;

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("FullName");
            dataTable.Rows.Add("James Bond");

            // Execute the mail merge
            doc.MailMerge.Execute(dataTable);

            // The result will not be visible in the document because the IF field is false, but the inner MERGEFIELD did indeed receive data
            doc.Save(ArtifactsDir + "MailMerge.UnconditionalMergeFieldsAndRegions.docx");

            if (doCountAllMergeFields)
                Assert.AreEqual("\u0013 IF 1 = 2 \"James Bond\"\u0014\u0015", doc.GetText().Trim());
            else
                Assert.AreEqual("\u0013 IF 1 = 2 \u0013 MERGEFIELD  FullName \u0014«FullName»\u0015\u0014\u0015", doc.GetText().Trim());
            //ExEnd
        }

        [TestCase(true, SectionStart.Continuous, SectionStart.Continuous)]
        [TestCase(true, SectionStart.NewColumn, SectionStart.NewColumn)]
        [TestCase(true, SectionStart.NewPage, SectionStart.NewPage)]
        [TestCase(true, SectionStart.EvenPage, SectionStart.EvenPage)]
        [TestCase(true, SectionStart.OddPage, SectionStart.OddPage)]
        [TestCase(false, SectionStart.Continuous, SectionStart.NewPage)]
        [TestCase(false, SectionStart.NewColumn, SectionStart.NewPage)]
        [TestCase(false, SectionStart.NewPage, SectionStart.NewPage)]
        [TestCase(false, SectionStart.EvenPage, SectionStart.EvenPage)]
        [TestCase(false, SectionStart.OddPage, SectionStart.OddPage)]
        public void RetainFirstSectionStart(bool isRetainFirstSectionStart, SectionStart sectionStart, SectionStart expected)
        {
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);
            
            builder.InsertField(" MERGEFIELD  FullName ");

            doc.FirstSection.PageSetup.SectionStart = sectionStart;
            doc.MailMerge.RetainFirstSectionStart = isRetainFirstSectionStart;

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("FullName");
            dataTable.Rows.Add("James Bond");

            doc.MailMerge.Execute(dataTable);

            foreach (Section section in doc.Sections)
                Assert.AreEqual(expected, section.PageSetup.SectionStart);
        }

        [Test]
        public void MailMergeSettings()
        {
            //ExStart
            //ExFor:Document.MailMergeSettings
            //ExFor:MailMergeCheckErrors
            //ExFor:MailMergeDataType
            //ExFor:MailMergeDestination
            //ExFor:MailMergeMainDocumentType
            //ExFor:MailMergeSettings
            //ExFor:MailMergeSettings.CheckErrors
            //ExFor:MailMergeSettings.Clone
            //ExFor:MailMergeSettings.Destination
            //ExFor:MailMergeSettings.DataType
            //ExFor:MailMergeSettings.DoNotSupressBlankLines
            //ExFor:MailMergeSettings.LinkToQuery
            //ExFor:MailMergeSettings.MainDocumentType
            //ExFor:MailMergeSettings.Odso
            //ExFor:MailMergeSettings.Query
            //ExFor:MailMergeSettings.ViewMergedData
            //ExFor:Odso
            //ExFor:Odso.Clone
            //ExFor:Odso.ColumnDelimiter
            //ExFor:Odso.DataSource
            //ExFor:Odso.DataSourceType
            //ExFor:Odso.FirstRowContainsColumnNames
            //ExFor:OdsoDataSourceType
            //ExSummary:Shows how to execute an Office Data Source Object mail merge with MailMergeSettings.
            // We'll create a simple document that will act as a destination for mail merge data
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.Write("Dear ");
            builder.InsertField("MERGEFIELD FirstName", "<FirstName>");
            builder.Write(" ");
            builder.InsertField("MERGEFIELD LastName", "<LastName>");
            builder.Writeln(": ");
            builder.InsertField("MERGEFIELD Message", "<Message>");

            // We will use an ASCII file as a data source
            // We can use any character we want as a delimiter, in this case we'll choose '|'
            // The delimiter character is selected in the ODSO settings of mail merge settings
            string[] lines = { "FirstName|LastName|Message",
                "John|Doe|Hello! This message was created with Aspose Words mail merge." };
            string dataSrcFilename = ArtifactsDir + "MailMerge.MailMergeSettings.DataSource.txt";

            File.WriteAllLines(dataSrcFilename, lines);

            // Set the data source, query and other things
            MailMergeSettings settings = doc.MailMergeSettings;
            settings.MainDocumentType = MailMergeMainDocumentType.MailingLabels;
            settings.CheckErrors = MailMergeCheckErrors.Simulate;
            settings.DataType = MailMergeDataType.Native;
            settings.DataSource = dataSrcFilename;
            settings.Query = "SELECT * FROM " + doc.MailMergeSettings.DataSource;
            settings.LinkToQuery = true;
            settings.ViewMergedData = true;

            Assert.AreEqual(MailMergeDestination.Default, settings.Destination);
            Assert.False(settings.DoNotSupressBlankLines);

            // Office Data Source Object settings
            Odso odso = settings.Odso;
            odso.DataSource = dataSrcFilename;
            odso.DataSourceType = OdsoDataSourceType.Text;
            odso.ColumnDelimiter = '|';
            odso.FirstRowContainsColumnNames = true;

            // ODSO/MailMergeSettings objects can also be cloned
            Assert.AreNotSame(odso, odso.Clone());
            Assert.AreNotSame(settings, settings.Clone());

            // The mail merge will be performed when this document is opened 
            doc.Save(ArtifactsDir + "MailMerge.MailMergeSettings.docx");
            //ExEnd

            settings = new Document(ArtifactsDir + "MailMerge.MailMergeSettings.docx").MailMergeSettings;

            Assert.AreEqual(MailMergeMainDocumentType.MailingLabels, settings.MainDocumentType);
            Assert.AreEqual(MailMergeCheckErrors.Simulate, settings.CheckErrors);
            Assert.AreEqual(MailMergeDataType.Native, settings.DataType);
            Assert.AreEqual(ArtifactsDir + "MailMerge.MailMergeSettings.DataSource.txt", settings.DataSource);
            Assert.AreEqual("SELECT * FROM " + doc.MailMergeSettings.DataSource, settings.Query);
            Assert.True(settings.LinkToQuery);
            Assert.True(settings.ViewMergedData);

            odso = settings.Odso;
            Assert.AreEqual(ArtifactsDir + "MailMerge.MailMergeSettings.DataSource.txt", odso.DataSource);
            Assert.AreEqual(OdsoDataSourceType.Text, odso.DataSourceType);
            Assert.AreEqual('|', odso.ColumnDelimiter);
            Assert.True(odso.FirstRowContainsColumnNames);
        }

        [Test]
        public void OdsoEmail()
        {
            //ExStart
            //ExFor:MailMergeSettings.ActiveRecord
            //ExFor:MailMergeSettings.AddressFieldName
            //ExFor:MailMergeSettings.ConnectString
            //ExFor:MailMergeSettings.MailAsAttachment
            //ExFor:MailMergeSettings.MailSubject
            //ExFor:MailMergeSettings.Clear
            //ExFor:Odso.TableName
            //ExFor:Odso.UdlConnectString
            //ExSummary:Shows how to execute a mail merge while connecting to an external data source.
            Document doc = new Document(MyDir + "Odso data.docx");
            TestOdsoEmail(doc); //ExSkip
            MailMergeSettings settings = doc.MailMergeSettings;

            Console.WriteLine($"Connection string:\n\t{settings.ConnectString}");
            Console.WriteLine($"Mail merge docs as attachment:\n\t{settings.MailAsAttachment}");
            Console.WriteLine($"Mail merge doc e-mail subject:\n\t{settings.MailSubject}");
            Console.WriteLine($"Column that contains e-mail addresses:\n\t{settings.AddressFieldName}");
            Console.WriteLine($"Active record:\n\t{settings.ActiveRecord}");

            Odso odso = settings.Odso;

            Console.WriteLine($"File will connect to data source located in:\n\t\"{odso.DataSource}\"");
            Console.WriteLine($"Source type:\n\t{odso.DataSourceType}");
            Console.WriteLine($"UDL connection string:\n\t{odso.UdlConnectString}");
            Console.WriteLine($"Table:\n\t{odso.TableName}");
            Console.WriteLine($"Query:\n\t{doc.MailMergeSettings.Query}");

            // We can clear the settings, which will take place during saving
            settings.Clear();

            doc.Save(ArtifactsDir + "MailMerge.OdsoEmail.docx");
            //ExEnd

            doc = new Document(ArtifactsDir + "MailMerge.OdsoEmail.docx");
            Assert.That(doc.MailMergeSettings.ConnectString, Is.Empty);
        }

        private void TestOdsoEmail(Document doc)
        {
            MailMergeSettings settings = doc.MailMergeSettings;

            Assert.False(settings.MailAsAttachment);
            Assert.AreEqual("test subject", settings.MailSubject);
            Assert.AreEqual("Email_Address", settings.AddressFieldName);
            Assert.AreEqual(66, settings.ActiveRecord);
            Assert.AreEqual("SELECT * FROM `Contacts` ", settings.Query);

            Odso odso = settings.Odso;

            Assert.AreEqual(settings.ConnectString, odso.UdlConnectString);
            Assert.AreEqual("Personal Folders|", odso.DataSource);
            Assert.AreEqual(OdsoDataSourceType.Email, odso.DataSourceType);
            Assert.AreEqual("Contacts", odso.TableName);
        }

        [Test]
        public void MailingLabelMerge()
        {
            //ExStart
            //ExFor:MailMergeSettings.DataSource
            //ExFor:MailMergeSettings.HeaderSource
            //ExSummary:Shows how to execute a mail merge while drawing data from a header and a data file.
            // Create a mailing label merge header file, which will consist of a table with one row 
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.StartTable();
            builder.InsertCell();
            builder.Write("FirstName");
            builder.InsertCell();
            builder.Write("LastName");
            builder.EndTable();

            doc.Save(ArtifactsDir + "MailMerge.MailingLabelMerge.Header.docx");

            // Create a mailing label merge date file, which will consist of a table with one row and the same amount of columns as 
            // the header table, which will determine the names for these columns
            doc = new Document();
            builder = new DocumentBuilder(doc);

            builder.StartTable();
            builder.InsertCell();
            builder.Write("John");
            builder.InsertCell();
            builder.Write("Doe");
            builder.EndTable();

            doc.Save(ArtifactsDir + "MailMerge.MailingLabelMerge.Data.docx");

            // Create a merge destination document with MERGEFIELDS that will accept data
            doc = new Document();
            builder = new DocumentBuilder(doc);

            builder.Write("Dear ");
            builder.InsertField("MERGEFIELD FirstName", "<FirstName>");
            builder.Write(" ");
            builder.InsertField("MERGEFIELD LastName", "<LastName>");

            // Configure settings to draw data and headers from other documents
            MailMergeSettings settings = doc.MailMergeSettings;

            // The "header" document contains column names for the data in the "data" document,
            // which will correspond to the names of our MERGEFIELDs
            settings.HeaderSource = ArtifactsDir + "MailMerge.MailingLabelMerge.Header.docx";
            settings.DataSource = ArtifactsDir + "MailMerge.MailingLabelMerge.Data.docx";

            // Configure the rest of the MailMergeSettings object
            settings.Query = "SELECT * FROM " + settings.DataSource;
            settings.MainDocumentType = MailMergeMainDocumentType.MailingLabels;
            settings.DataType = MailMergeDataType.TextFile;
            settings.LinkToQuery = true;
            settings.ViewMergedData = true;

            // The mail merge will be performed when this document is opened 
            doc.Save(ArtifactsDir + "MailMerge.MailingLabelMerge.docx");
            //ExEnd

            Assert.AreEqual("FirstName\aLastName\a\a",
                new Document(ArtifactsDir + "MailMerge.MailingLabelMerge.Header.docx").
                    GetChild(NodeType.Table, 0, true).GetText().Trim());

            Assert.AreEqual("John\aDoe\a\a",
                new Document(ArtifactsDir + "MailMerge.MailingLabelMerge.Data.docx").
                    GetChild(NodeType.Table, 0, true).GetText().Trim());

            doc = new Document(ArtifactsDir + "MailMerge.MailingLabelMerge.docx");

            Assert.AreEqual(2, doc.Range.Fields.Count);

            settings = doc.MailMergeSettings;

            Assert.AreEqual(ArtifactsDir + "MailMerge.MailingLabelMerge.Header.docx", settings.HeaderSource);
            Assert.AreEqual(ArtifactsDir + "MailMerge.MailingLabelMerge.Data.docx", settings.DataSource);
            Assert.AreEqual("SELECT * FROM " + settings.DataSource, settings.Query);
            Assert.AreEqual(MailMergeMainDocumentType.MailingLabels, settings.MainDocumentType);
            Assert.AreEqual(MailMergeDataType.TextFile, settings.DataType);
            Assert.True(settings.LinkToQuery);
            Assert.True(settings.ViewMergedData);
        }

        [Test]
        public void OdsoFieldMapDataCollection()
        {
            //ExStart
            //ExFor:Odso.FieldMapDatas
            //ExFor:OdsoFieldMapData
            //ExFor:OdsoFieldMapData.Clone
            //ExFor:OdsoFieldMapData.Column
            //ExFor:OdsoFieldMapData.MappedName
            //ExFor:OdsoFieldMapData.Name
            //ExFor:OdsoFieldMapData.Type
            //ExFor:OdsoFieldMapDataCollection
            //ExFor:OdsoFieldMapDataCollection.Add(OdsoFieldMapData)
            //ExFor:OdsoFieldMapDataCollection.Clear
            //ExFor:OdsoFieldMapDataCollection.Count
            //ExFor:OdsoFieldMapDataCollection.GetEnumerator
            //ExFor:OdsoFieldMapDataCollection.Item(Int32)
            //ExFor:OdsoFieldMapDataCollection.RemoveAt(Int32)
            //ExFor:OdsoFieldMappingType
            //ExSummary:Shows how to access the collection of data that maps data source columns to merge fields.
            Document doc = new Document(MyDir + "Odso data.docx");

            // This collection defines how columns from an external data source will be mapped to predefined MERGEFIELD,
            // ADDRESSBLOCK and GREETINGLINE fields during a mail merge
            OdsoFieldMapDataCollection dataCollection = doc.MailMergeSettings.Odso.FieldMapDatas;
            Assert.AreEqual(30, dataCollection.Count);

            using (IEnumerator<OdsoFieldMapData> enumerator = dataCollection.GetEnumerator())
            {
                int index = 0;
                while (enumerator.MoveNext())
                {
                    Console.WriteLine($"Field map data index {index++}, type \"{enumerator.Current.Type}\":");

                    Console.WriteLine(
                        enumerator.Current.Type != OdsoFieldMappingType.Null
                            ? $"\tColumn \"{enumerator.Current.Name}\", number {enumerator.Current.Column} mapped to merge field \"{enumerator.Current.MappedName}\"."
                            : "\tNo valid column to field mapping data present.");
                }
            }

            // Elements of the collection can be cloned
            Assert.AreNotEqual(dataCollection[0], dataCollection[0].Clone());

            // The collection can have individual entries removed or be cleared like this
            dataCollection.RemoveAt(0);
            Assert.AreEqual(29, dataCollection.Count); //ExSkip
            dataCollection.Clear();
            Assert.AreEqual(0, dataCollection.Count); //ExSkip
            //ExEnd
        }

        [Test]
        public void OdsoRecipientDataCollection()
        {
            //ExStart
            //ExFor:Odso.RecipientDatas
            //ExFor:OdsoRecipientData
            //ExFor:OdsoRecipientData.Active
            //ExFor:OdsoRecipientData.Clone
            //ExFor:OdsoRecipientData.Column
            //ExFor:OdsoRecipientData.Hash
            //ExFor:OdsoRecipientData.UniqueTag
            //ExFor:OdsoRecipientDataCollection
            //ExFor:OdsoRecipientDataCollection.Add(OdsoRecipientData)
            //ExFor:OdsoRecipientDataCollection.Clear
            //ExFor:OdsoRecipientDataCollection.Count
            //ExFor:OdsoRecipientDataCollection.GetEnumerator
            //ExFor:OdsoRecipientDataCollection.Item(Int32)
            //ExFor:OdsoRecipientDataCollection.RemoveAt(Int32)
            //ExSummary:Shows how to access the collection of data that designates merge data source records to be excluded from a merge.
            Document doc = new Document(MyDir + "Odso data.docx");

            // Records in this collection that do not have the "Active" flag set to true will be excluded from the mail merge
            OdsoRecipientDataCollection dataCollection = doc.MailMergeSettings.Odso.RecipientDatas;

            Assert.AreEqual(70, dataCollection.Count);

            using (IEnumerator<OdsoRecipientData> enumerator = dataCollection.GetEnumerator())
            {
                int index = 0;
                while (enumerator.MoveNext())
                {
                    Console.WriteLine(
                        $"Odso recipient data index {index++} will {(enumerator.Current.Active ? "" : "not ")}be imported upon mail merge.");
                    Console.WriteLine($"\tColumn #{enumerator.Current.Column}");
                    Console.WriteLine($"\tHash code: {enumerator.Current.Hash}");
                    Console.WriteLine($"\tContents array length: {enumerator.Current.UniqueTag.Length}");
                }
            }

            // Elements of the collection can be cloned
            Assert.AreNotEqual(dataCollection[0], dataCollection[0].Clone());

            // The collection can have individual entries removed or be cleared like this
            dataCollection.RemoveAt(0);
            Assert.AreEqual(69, dataCollection.Count); //ExSkip
            dataCollection.Clear();
            Assert.AreEqual(0, dataCollection.Count); //ExSkip
            //ExEnd
        }

        [Test]
        public void ChangeFieldUpdateCultureSource()
        {
            //ExStart
            //ExFor:Document.FieldOptions
            //ExFor:FieldOptions
            //ExFor:FieldOptions.FieldUpdateCultureSource
            //ExFor:FieldUpdateCultureSource
            //ExSummary:Shows how to specify where the culture used for date formatting during a field update or mail merge is sourced from.
            Document doc = new Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // Insert two merge fields with German locale.
            builder.Font.LocaleId = 1031;
            builder.InsertField("MERGEFIELD Date1 \\@ \"dddd, d MMMM yyyy\"");
            builder.Write(" - ");
            builder.InsertField("MERGEFIELD Date2 \\@ \"dddd, d MMMM yyyy\"");

            // Set the current culture to US English after preserving its original value in a variable.
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // This merge will use the current thread's culture to format the date, which will be US English.
            doc.MailMerge.Execute(new[] { "Date1" }, new object[] { new DateTime(2020, 1, 01) });

            // Configure the next merge to source its culture value from the field code. The value of that culture will be German.
            doc.FieldOptions.FieldUpdateCultureSource = FieldUpdateCultureSource.FieldCode;
            doc.MailMerge.Execute(new[] { "Date2" }, new object[] { new DateTime(2020, 1, 01) });

            // The first merge result contains a date formatted in English, while the second one is in German.
            Assert.AreEqual("Wednesday, 1 January 2020 - Mittwoch, 1 Januar 2020", doc.Range.Text.Trim());

            // Restore the original culture.
            Thread.CurrentThread.CurrentCulture = currentCulture;
            //ExEnd
        }
    }
}