using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace TagsCloudVisualization.Tests
{
    public class RectangleStorageTests
    {
        private RectangleStorage structStorage;
        private Rectangle defaulRectangle;
        [SetUp]
        public void SetUp()
        {
            structStorage = new RectangleStorage();
            defaulRectangle = new(new(2, 2), new(2, 2));
        }

        [Test]
        public void GetRectangles_ShouldGetAllRectangle()
        {
            structStorage.Add(defaulRectangle);
            structStorage.Add(defaulRectangle);

            structStorage.GetAll().Should().HaveCount(2);
        }

        [Test]
        public void AddRectangle_ShouldGetIdForRectangle()
        {
            var id = structStorage.Add(defaulRectangle);

            structStorage.GetById(id).Should().Be(defaulRectangle);
        }

        [Test]
        public void ChangeRectangle_ShouldChangeRectangleByIndex()
        {
            var id = structStorage.Add(defaulRectangle);
            var rectangleForChange = structStorage.GetById(id);

            rectangleForChange.Size = new Size(1, 1);

            structStorage.GetById(id).Size.Should().Be(rectangleForChange.Size);
        }
    }
}
