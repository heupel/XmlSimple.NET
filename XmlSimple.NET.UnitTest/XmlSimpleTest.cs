﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmlSimple.NET.UnitTest
{
    /// <summary>
    /// Unit Tests for XmlSimple class
    /// </summary>
    [TestClass]
    public class XmlSimpleTest
    {
        private string libraryXmlString = @"<Library Name=""Covington""><Address><Street>27100 164th Ave. S.E.</Street><City>Covington</City><State>WA</State><Zip>98042</Zip></Address><Books><Book Title=""Soon Will Come the Light"" Author=""Tom McKean""></Book><Book Title=""Fall of Giants"" Author=""Ken Follett"" /></Books></Library>";
        
        [TestMethod]
        public void XmlInWithStringOfXmlAndDefaultOptions()
        {
            dynamic library = XmlSimple.XmlIn(this.libraryXmlString);

            Assert.IsInstanceOfType(library, typeof(XmlSimple));
            Assert.AreEqual("Covington", library["name"]);
            Assert.IsFalse(library.ContainsKey("@name")); // Ensure attributes don't have prefix
            Assert.AreEqual("Covington", library.Name);
            Assert.AreEqual("Covington", library.nAMe);   // Support mixed-case
            Assert.AreEqual("27100 164th Ave. S.E.", library.Address.Street);
            Assert.IsTrue(library.ContainsKey("Books"));
            Assert.AreSame(library["Books"]["Book"][1], library.Books.Book[1]);
            Assert.AreEqual("Soon Will Come the Light", library.Books.Book[0].Title);
        }

        [TestMethod]
        public void XmlInWithStringOfXmlAndAttrPrefixOptionSetToTrue()
        {
            dynamic library = XmlSimple.XmlIn(this.libraryXmlString, new Options { AttrPrefix = true });

            Assert.IsInstanceOfType(library, typeof(XmlSimple));
            Assert.AreEqual("Covington", library["@name"]); // Ensure attribute has prefix
            Assert.AreEqual("Covington", library["name"]);  // Support missing prefix even though attributes have them
            Assert.AreEqual("Covington", library.Name);
            Assert.AreEqual("Covington", library.nAMe);
            Assert.AreEqual("27100 164th Ave. S.E.", library.Address.Street);
            Assert.IsTrue(library.ContainsKey("Books"));
            Assert.AreSame(library["Books"]["Book"][1], library.Books.Book[1]);
            Assert.AreEqual("Soon Will Come the Light", library.Books.Book[0].Title);
            Assert.AreEqual("Soon Will Come the Light", library.Books.Book[0]["@tiTle"]);
        }

        [ExpectedException(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException))]
        [TestMethod]
        public void XmlInWithStringOfXmlAndNoAttrOptionSetToTrue()
        {
            dynamic library = XmlSimple.XmlIn(this.libraryXmlString, new Options { NoAttr = true });

            Assert.IsInstanceOfType(library, typeof(XmlSimple));
            Assert.IsFalse(library.ContainsKey("Name"));
            Assert.AreEqual("27100 164th Ave. S.E.", library.Address.Street);
            Assert.IsTrue(library.ContainsKey("Books"));
            Assert.AreSame(library["Books"]["Book"][1], library.Books.Book[1]);
            
            Assert.IsInstanceOfType(library.Books.Book[0], typeof(XmlSimple));
 
            // The next one should trigger the ExpectedExceptionAttribute of this test method
            string title = library.Books.Book[0].Title;
        }

        [TestMethod]
        public void XmlInWithStringOfXmlAndForceArrayOptionSetToTrue()
        {
            dynamic library = XmlSimple.XmlIn(this.libraryXmlString, new Options { ForceArray = true });

            Assert.IsNotNull(library.Address as IEnumerable<object>);
            Assert.IsNotNull(library.Address[0].Street as IEnumerable<object>);
            Assert.IsNotNull(library.Books as IEnumerable<object>);
            Assert.AreEqual("27100 164th Ave. S.E.", library.Address[0].Street[0]);
            Assert.AreSame(library["Books"][0]["Book"][1], library.Books[0].Book[1]);
            Assert.IsInstanceOfType(library.Books[0].Book[0], typeof(XmlSimple));
        }


        [TestMethod]
        public void XmlInWithStringOfXmlAndForceArrayOptionSetToListOfAddressAndZip()
        {
            dynamic library = XmlSimple.XmlIn(this.libraryXmlString, new Options { ForceArray = new string[] { "Address", "Zip" } });

            Assert.IsNotNull(library.Address as IEnumerable<object>);
            Assert.IsInstanceOfType(library.Address[0].Street, typeof(string));
            Assert.IsNotNull(library.Address[0].Zip as IEnumerable<object>);
            Assert.IsInstanceOfType(library.Books, typeof(XmlSimple));
            Assert.AreEqual("98042", library.Address[0].Zip[0]);

            Assert.AreEqual("27100 164th Ave. S.E.", library.Address[0].Street);
            Assert.AreSame(library["Books"]["Book"][1], library.Books.Book[1]);
            Assert.IsInstanceOfType(library.Books.Book[0], typeof(XmlSimple));
        }

        [Ignore]
        [TestMethod]
        public void TryXmlSimpleWithYahoo()
        {
            /* THE RUBY VERSION
            require 'net/http'
            require 'rubygems'
            require 'xmlsimple'

            url = 'http://api.search.yahoo.com/WebSearchService/V1/webSearch?appid=YahooDemo&query=madonna&results=2'
            xml_data = Net::HTTP.get_response(URI.parse(url)).body

            data = XmlSimple.xml_in(xml_data)

            data['Result'].each do |item|
               item.sort.each do |k, v|
                  if ["Title", "Url"].include? k
                     print "#{v[0]}" if k=="Title"
                     print " => #{v[0]}\n" if k=="Url"
                  end
               end
            end
            */

            // The XmlSimple.NET version
            var url = "http://api.search.yahoo.com/WebSearchService/V1/webSearch?appid=YahooDemo&query=madonna&results=2";
            //var xmlData = new System.Net.WebClient().DownloadString(url);

            dynamic data = XmlSimple.XmlIn(url);

            dynamic dataAttrPrefix = XmlSimple.XmlIn(url, new Options() { AttrPrefix = true });

            // The next two lines are equivalent - notice the use of mixed case
            data.Result.ForEach(new Action<dynamic>(result => Console.WriteLine("{0} => {1}", result.Title, result.URl)));
            data["result"].ForEach(new Action<dynamic>(result => Console.WriteLine("{0} => {1}", result["TiTle"], result["URL"])));
        }
    }
}
