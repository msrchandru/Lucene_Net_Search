// ***********************************************************************
// Assembly         : SearchTextinDB
// Author           : Dharani
// Created          : 04-16-2012
//
// Last Modified By : Dharani
// Last Modified On : 01-10-2013
//
// Last Modified By : Chandra Sekaran
// Last Modified On : 27-09-2013
//
// ***********************************************************************
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.Highlight;
using System.Diagnostics;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using System.Data;
using System.Data.SqlClient;
using SpellChecker.Net.Search.Spell;
using System.Configuration;
using System.IO;


namespace SearchingTextinDB
{
    public partial class _Default : System.Web.UI.Page
    {
        string strcon = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        /// <summary>
        /// Creating an object to store the searched data
        /// </summary>
        public class SearchResults
        {
            public string PageName { get; set; }
            public string Tag { get; set; }
            public string ContentText { get; set; }
            public int Priority { get; set; }
        }
        /// <summary>
        /// Set value for minimum value for prefix match
        /// </summary>
        public enum MinValue
        {
            MinPrefexvalue = 5
        }

        #region Indexing methods
        // The query fetch all person details
        public DataSet GetPersons()
        {
            String sqlQuery = @"SELECT [PageName],[Tag],[ContentText],[Priority] FROM [dbo].[tblCrawlerData]";

            return GetDataSet(sqlQuery);
        }

        // Returns the dataset
        public DataSet GetDataSet(string sqlQuery)
        {
            DataSet ds = new DataSet();
            SqlConnection sqlCon = new SqlConnection(strcon);
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.Connection = sqlCon;
            sqlCmd.CommandType = CommandType.Text;
            sqlCmd.CommandText = sqlQuery;
            SqlDataAdapter sqlAdap = new SqlDataAdapter(sqlCmd);
            sqlAdap.Fill(ds);
            return ds;
        }

        // Creates the lucene.net index with person details
        public void CreatePersonsIndex(DataSet ds)
        {
            //Specify the index file location where the indexes are to be stored
            string indexFileLocation = @"D:\Lucene.Net\Data\Persons";
            Lucene.Net.Store.Directory dir = Lucene.Net.Store.FSDirectory.GetDirectory(indexFileLocation, true);
            IndexWriter indexWriter = new IndexWriter(dir, new StandardAnalyzer(), true);
            indexWriter.SetRAMBufferSizeMB(10.0);
            indexWriter.SetUseCompoundFile(false);
            indexWriter.SetMaxMergeDocs(10000);
            indexWriter.SetMergeFactor(100);

            if (ds.Tables[0] != null)
            {
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        //Create the Document object
                        Document doc = new Document();
                        foreach (DataColumn dc in dt.Columns)
                        {
                            //Populate the document with the column name and value from our query
                            doc.Add(new Field(dc.ColumnName, dr[dc.ColumnName].ToString(), Field.Store.YES, Field.Index.TOKENIZED));
                        }
                        // Write the Document to the catalog
                        indexWriter.AddDocument(doc);
                    }
                }
            }
            // Close the writer
            indexWriter.Close();
        }
        #endregion

        #region Searching Methods
        /// <summary>
        /// for simple searching
        /// </summary>
        /// <param name="searchString"></param>
        public void SearchPersons(string searchString)
        {
            // Results are collected as a List
            List<SearchResults> Searchresults = new List<SearchResults>();

            // Specify the location where the index files are stored
            string indexFileLocation = @"D:\Lucene.Net\Data\Persons";
            Lucene.Net.Store.Directory dir = Lucene.Net.Store.FSDirectory.GetDirectory(indexFileLocation);
            // specify the search fields, lucene search in multiple fields
            string[] searchfields = new string[] { "ContentText" };
            IndexSearcher indexSearcher = new IndexSearcher(dir);

            // Making a boolean query for searching and get the searched hits
            BooleanQuery objbool = QueryMaker(searchString, searchfields);

            var hits = indexSearcher.Search(objbool); // ~ symbol is used for fuzzy search. * for wildcard search

            List<SearchResults> searchlist = new List<SearchResults>();
            SearchResults result = null;
            //add to list
            for (int i = 0; i < hits.Length(); i++)
            {
                result = new SearchResults();
                result.PageName = hits.Doc(i).GetField("PageName").StringValue();
                result.Tag = hits.Doc(i).GetField("Tag").StringValue();
                result.ContentText = hits.Doc(i).GetField("ContentText").StringValue();
                result.Priority = Convert.ToInt32(hits.Doc(i).GetField("Priority").StringValue());
                searchlist.Add(result);
            }
            //sort by priority
            searchlist = searchlist.OrderBy(x => x.Priority).ToList();

            indexSearcher.Close();
            GridView1.DataSource = searchlist;
            GridView1.DataBind();
        }

        /// <summary>
        /// Making the query for simple search
        /// </summary>
        /// <param name="searchString">text for search</param>
        /// <param name="searchfields">passing fields for search</param>
        /// <returns></returns>
        public BooleanQuery QueryMaker(string searchString, string[] searchfields)
        {
            var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, searchfields, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));

            var finalQuery = new BooleanQuery();

            string searchText;
            searchText = searchString.Replace("+", "");
            searchText = searchText.Replace("\"", "");
            searchText = searchText.Replace("\'", "");
            searchText = searchText.Replace("~", "");

            //Split the search string into separate search terms by word
            string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string term in terms)
            {

                //if (Typeofwords.iWithallwords == typeofword)
                //    finalQuery.Add(parser.Parse(term.Replace("*", "") + "*"), BooleanClause.Occur.SHOULD);
                //else if (Typeofwords.iexactphase == typeofword)
                //    finalQuery.Add(parser.Parse(term.Replace("*", "") + "*"), BooleanClause.Occur.MUST);
                //else if (Typeofwords.atleastoneword == typeofword)
                //    finalQuery.Add(parser.Parse(term.Replace("*", "") + "*"), BooleanClause.Occur.SHOULD);
                //else if (Typeofwords.withoutwords == typeofword) { }
                //else
                finalQuery.Add(parser.Parse(term.Replace("*", "") + "*"), BooleanClause.Occur.SHOULD);

                ////for without word
                //if (Typeofwords.withoutwords == typeofword)
                //{
                //    finalQuery.Add(new BooleanClause(new MatchAllDocsQuery(), BooleanClause.Occur.SHOULD));
                //    finalQuery.Add(new BooleanClause(new TermQuery(new Term("ContentText", term)), BooleanClause.Occur.MUST_NOT));

                //}
                //else
                //{
                //    Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f, 4);
                //    finalQuery.Add(query, BooleanClause.Occur.SHOULD);
                //}
                Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f,Convert.ToInt32(MinValue.MinPrefexvalue));
                finalQuery.Add(query, BooleanClause.Occur.SHOULD);
            }

            return finalQuery;
        }

        #region Advance search Methods

        /// <summary>
        /// For Advance searching- with group search
        /// </summary>
        public void SearchPersons_Multiple()
        {
            // Results are collected as a List
            List<SearchResults> Searchresults = new List<SearchResults>();

            // Specify the location where the index files are stored
            string indexFileLocation = @"D:\Lucene.Net\Data\Persons";
            Lucene.Net.Store.Directory dir = Lucene.Net.Store.FSDirectory.GetDirectory(indexFileLocation);
            // specify the search fields, lucene search in multiple fields
            string[] searchfields = new string[] { "ContentText" };
            IndexSearcher indexSearcher = new IndexSearcher(dir);

            // Making a boolean query for searching and get the searched hits

            BooleanQuery objbool = QueryMaker_Multiple(searchfields);

            var hits = indexSearcher.Search(objbool); // ~ symbol is used for fuzzy search. * for wildcard search

            List<SearchResults> searchlist = new List<SearchResults>();
            SearchResults result = null;

            for (int i = 0; i < hits.Length(); i++)
            {
                result = new SearchResults();
                result.PageName = hits.Doc(i).GetField("PageName").StringValue();
                result.Tag = hits.Doc(i).GetField("Tag").StringValue();
                result.ContentText = hits.Doc(i).GetField("ContentText").StringValue();
                result.Priority = Convert.ToInt32(hits.Doc(i).GetField("Priority").StringValue());

                searchlist.Add(result);

            }
            //sort by priority
            searchlist = searchlist.OrderBy(x => x.Priority).ToList();

            indexSearcher.Close();
            GridView1.DataSource = searchlist;
            GridView1.DataBind();
        }

        /// <summary>
        /// Making the query for multiple search
        /// </summary>
        /// <param name="searchfields"></param>
        /// <returns></returns>
        public BooleanQuery QueryMaker_Multiple(string[] searchfields)
        {
            //var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, searchfields, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
            var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, searchfields[0], new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));

            var finalQuery = new BooleanQuery();

            //for Text with all words
            if (txtWords.Text != string.Empty)
            {
                string searchText = txtWords.Text.Trim();
                searchText = searchText.Replace("+", "");
                searchText = searchText.Replace("\"", "");
                searchText = searchText.Replace("\'", "");
                searchText = searchText.Replace("~", "");

                //Split the search string into separate search terms by word
                string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var finalQuery_sub = new BooleanQuery();

                foreach (string term in terms)
                {
                    Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f, Convert.ToInt32(MinValue.MinPrefexvalue));
                    finalQuery_sub.Add(query, BooleanClause.Occur.SHOULD);
                }

                finalQuery.Add(finalQuery_sub, BooleanClause.Occur.SHOULD);

            }
            // with exact phrase
            if (txtPhrase.Text != string.Empty)
            {
                string searchText = txtPhrase.Text.Trim();
                searchText = searchText.Replace("+", "");
                searchText = searchText.Replace("\"", "");
                searchText = searchText.Replace("\'", "");
                searchText = searchText.Replace("~", "");

                //Split the search string into separate search terms by word
                string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var finalQuery_sub = new BooleanQuery();

                foreach (string term in terms)
                {
                    finalQuery_sub.Add(parser.Parse(term.Replace("*", "") + ""), BooleanClause.Occur.MUST);
                    //Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f, 4);
                    //finalQuery.Add(query, BooleanClause.Occur.MUST);
                }
                finalQuery.Add(finalQuery_sub, BooleanClause.Occur.SHOULD);
            }
            // for atleast one word
            if (txtLeastWords.Text != string.Empty)
            {
                string searchText = txtLeastWords.Text.Trim();
                searchText = searchText.Replace("+", "");
                searchText = searchText.Replace("\"", "");
                searchText = searchText.Replace("\'", "");
                searchText = searchText.Replace("~", "");

                //Split the search string into separate search terms by word
                string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var finalQuery_sub = new BooleanQuery();

                foreach (string term in terms)
                {
                    //  finalQuery.Add(parser.Parse(term.Replace("*", "") + "*"), BooleanClause.Occur.SHOULD);
                    Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f, Convert.ToInt32(MinValue.MinPrefexvalue));
                    finalQuery_sub.Add(query, BooleanClause.Occur.SHOULD);
                }
                finalQuery.Add(finalQuery_sub, BooleanClause.Occur.SHOULD);
            }
            // for "without word"
            if (txtWithoutWords.Text != string.Empty)
            {
                string searchText = txtWithoutWords.Text.Trim();
                searchText = searchText.Replace("+", "");
                searchText = searchText.Replace("\"", "");
                searchText = searchText.Replace("\'", "");
                searchText = searchText.Replace("~", "");

                //Split the search string into separate search terms by word
                string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string term in terms)
                {

                    finalQuery.Add(parser.Parse(term.Replace("*", "") + ""), BooleanClause.Occur.MUST_NOT);
                    //previous code- to get all data then remove word from that list.
                    //finalQuery.Add(new BooleanClause(new MatchAllDocsQuery(), BooleanClause.Occur.SHOULD));
                    //finalQuery.Add(new BooleanClause(new TermQuery(new Term("ContentText", term)), BooleanClause.Occur.MUST_NOT));
                }
            }

            return finalQuery;
        }
        #endregion

        #endregion

        #region Page events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                idAdvance.Visible = false;
                idSimple.Visible = true;
            }
        }

        protected void btnCreateIndex_Click(object sender, EventArgs e)
        {
            CreatePersonsIndex(GetPersons());
        }

        // Calling the search function on button click   
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            //call search method
            SearchPersons(txtSearchval.Text);
        }
        //for advance search
        protected void btnWordSearch_Click(object sender, EventArgs e)
        {
            if (txtWords.Text == string.Empty && txtPhrase.Text == string.Empty && txtLeastWords.Text == string.Empty && txtWithoutWords.Text == string.Empty)
            {
                return;
            }
            else
            {//alteast one word in entered in the group
                SearchPersons_Multiple();
            }

        }

        protected void lnkbtnSimpleSearch_Click(object sender, EventArgs e)
        {
            idAdvance.Visible = false;
            idSimple.Visible = true;
        }

        protected void lnbtnAdvance_Click(object sender, EventArgs e)
        {
            idAdvance.Visible = true;
            idSimple.Visible = false;
        }
        #endregion
    }

}
