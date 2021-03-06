﻿using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlickrNet;

namespace FlickrNetTest
{
    /// <summary>
    /// Summary description for PhotoSearchOptionsTests
    /// </summary>
    [TestClass]
    public class PhotoSearchOptionsTests
    {
        public PhotoSearchOptionsTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void PhotoSearchOptionsCalculateSlideshowUrlBasicTest()
        {
            PhotoSearchOptions o = new PhotoSearchOptions();

            o.Text = "kittens";
            o.InGallery = true;

            string url = o.CalculateSlideshowUrl();

            Assert.IsNotNull(url);

            string expected = "http://www.flickr.com/show.gne?api_method=flickr.photos.search&method_params=text|kittens;in_gallery|1";

            Assert.AreEqual<string>(expected, url);

        }

        [TestMethod]
        public void PhotoSearchExtrasViews()
        {
            PhotoSearchOptions o = new PhotoSearchOptions();
            o.Tags = "kittens";
            o.Extras = PhotoSearchExtras.Views;

            var photos = TestData.GetInstance().PhotosSearch(o);

            foreach (var photo in photos)
            {
                Assert.IsTrue(photo.Views.HasValue);
            }

        }
    }
}
