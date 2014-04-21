﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;
namespace MetadataClient
{
    public class MetadataJobFacts
    {
        [Fact]
        public void AddPackageAssertions()
        {
            // Arrange
            var packageAssertions = new List<PackageAssertion>();
            var packageOwnerAssertions = new List<PackageOwnerAssertion>();

            var p1 = new PackageAssertion("A", "1.0.0", true, null, true, DateTime.MinValue, DateTime.MinValue);
            packageAssertions.Add(p1);

            var o1 = new PackageOwnerAssertion("A", "1.0.0", "user1", true);
            packageOwnerAssertions.Add(o1);

            var expectedAssertionsArray = JArray.Parse(@"[
  {
    'packageId': 'A',
    'version': '1.0.0',
    'exists': true,
    'nupkg': null,
    'listed': true,
    'created': '0001-01-01T00:00:00',
    'published': '0001-01-01T00:00:00',
    'owners': [
      {
        'username': 'user1',
        'exists': true
      }
    ]
  }
]");

            // Act
            var actualAssertionsArray = MetadataJob.GetJArrayAssertions(packageAssertions, packageOwnerAssertions);

            // Arrange
            Assert.Equal(expectedAssertionsArray, actualAssertionsArray);
        }

        [Fact]
        public void EditPackageAssertions()
        {
            // Arrange
            var packageAssertions = new List<PackageAssertion>();
            var packageOwnerAssertions = new List<PackageOwnerAssertion>();

            var p1 = new PackageAssertion("A", "1.0.0", true, null, true, DateTime.MinValue, DateTime.MinValue);
            packageAssertions.Add(p1);

            var expectedAssertionsArray = JArray.Parse(@"[
  {
    'packageId': 'A',
    'version': '1.0.0',
    'exists': true,
    'nupkg': null,
    'listed': true,
    'created': '0001-01-01T00:00:00',
    'published': '0001-01-01T00:00:00',
  }
]");

            // Act
            var actualAssertionsArray = MetadataJob.GetJArrayAssertions(packageAssertions, packageOwnerAssertions);

            // Arrange
            Assert.Equal(expectedAssertionsArray, actualAssertionsArray);
        }

        [Fact]
        public void DeletePackageAssertions()
        {
            // Arrange
            var packageAssertions = new List<PackageAssertion>();
            var packageOwnerAssertions = new List<PackageOwnerAssertion>();
            var p1 = new PackageAssertion("A", "1.0.0", false);
            packageAssertions.Add(p1);

            var expectedAssertionsArray = JArray.Parse(@"[
  {
    'packageId': 'A',
    'version': '1.0.0',
    'exists': false
  }
]");

            // Act
            var actualAssertionsArray = MetadataJob.GetJArrayAssertions(packageAssertions, packageOwnerAssertions);
            Assert.Equal(expectedAssertionsArray, actualAssertionsArray);
        }

        [Fact]
        public void AddRemoveOwnerAssertions()
        {
            // Arrange
            var packageAssertions = new List<PackageAssertion>();
            var packageOwnerAssertions = new List<PackageOwnerAssertion>();
            var o1 = new PackageOwnerAssertion("A", "1.0.0", "user1", true);
            var o2 = new PackageOwnerAssertion("A", "1.0.0", "user2", false);
            packageOwnerAssertions.Add(o1);
            packageOwnerAssertions.Add(o2);
            var expectedAssertionsArray = JArray.Parse(@"[
  {
    'packageId': 'A',
    'version': '1.0.0',
    'exists': true,
    'owners': [
      {
        'username': 'user1',
        'exists': true
      },
      {
        'username': 'user2',
        'exists': false
      }
    ]
  }
]");
            // Act
            var actualAssertionsArray = MetadataJob.GetJArrayAssertions(packageAssertions, packageOwnerAssertions);
            Assert.Equal(expectedAssertionsArray, actualAssertionsArray);
        }

        [Fact]
        public void RenameOwnerAssertions()
        {
            // Arrange
            var packageAssertions = new List<PackageAssertion>();
            var packageOwnerAssertions = new List<PackageOwnerAssertion>();
            var o1 = new PackageOwnerAssertion("A", "1.0.0", "user1", false);
            var o2 = new PackageOwnerAssertion("A", "1.0.0", "newuser1", true);
            var o3 = new PackageOwnerAssertion("A", "2.0.0", "user1", false);
            var o4 = new PackageOwnerAssertion("A", "2.0.0", "newuser1", true);
            packageOwnerAssertions.Add(o1);
            packageOwnerAssertions.Add(o2);
            packageOwnerAssertions.Add(o3);
            packageOwnerAssertions.Add(o4);

            var expectedAssertionsArray = JArray.Parse(@"[
  {
    'packageId': 'A',
    'version': '1.0.0',
    'exists': true,
    'owners': [
      {
        'username': 'user1',
        'exists': false
      },
      {
        'username': 'newuser1',
        'exists': true
      }
    ]
  },{
    'packageId': 'A',
    'version': '2.0.0',
    'exists': true,
    'owners': [
      {
        'username': 'user1',
        'exists': false
      },
      {
        'username': 'newuser1',
        'exists': true
      }
    ]
  }
]");
            // Act
            var actualAssertionsArray = MetadataJob.GetJArrayAssertions(packageAssertions, packageOwnerAssertions);
            Assert.Equal(expectedAssertionsArray, actualAssertionsArray);
        }
    }
}