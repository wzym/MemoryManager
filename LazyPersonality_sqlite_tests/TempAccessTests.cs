using System;
using System.Globalization;
using System.IO;
using System.Linq;
using LazyPersonality.Domain;
using Xunit;

namespace LazyPersonality.Sqlite.Tests
{
    public class TempAccessTests : IDisposable
    {
        private readonly DbService _dbService;

        public TempAccessTests()
        {
            Dispose();
            DbCreator.CreateTables().Wait();
            _dbService = new DbService();
        }

        [Fact]
        public void MemoryUnitAddingReading()
        {
            var notReadRecord = new MemoryUnit
            {
                Id = -9,
                CreationDate = DateTime.Now,
                Content = "Тестовый контент",
                Link = "пустая ссылка"
            };
            _dbService.AddMemUnit(notReadRecord).Wait();
            var readRecord = _dbService.GetMemesAsync().Result.ToArray();
            Assert.True(readRecord.Length == 1);
            Assert.True(readRecord[0].Content == notReadRecord.Content);
            Assert.True(readRecord[0].Link == notReadRecord.Link);
            Assert.True(readRecord[0].CreationDate.ToString(CultureInfo.InvariantCulture) 
                        == notReadRecord.CreationDate.ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void AssociationAddingReading()
        {
            var memoryUnit = new MemoryUnit
            {
                CreationDate = DateTime.Now,
                Content = "d",
                Link = string.Empty
            };
            _dbService.AddMemUnit(memoryUnit).Wait();
            _dbService.AddMemUnit(memoryUnit).Wait();
            var records = _dbService.GetMemesAsync().Result.ToArray();
            const string assTag = "aTag";
            _dbService.AddAssociationTagAsync(assTag).Wait();
            _dbService.BindUnitsAsync(records[0], records[1], assTag, DateTime.Now).Wait();

            var zeroAssociation = _dbService.GetAssociatedAsync(records[0]).Result.First();
            Assert.Equal(zeroAssociation.Apex2.Id, records[1].Id);
            var firstAssociation = _dbService.GetAssociatedAsync(records[1]).Result.First();
            Assert.Equal(zeroAssociation.Apex1.Id, records[0].Id);
            Assert.Equal(zeroAssociation.AssociationTag, firstAssociation.AssociationTag);
        }

        [Fact]
        public void TagAddingReading()
        {
            const string tag = "Тэг тестовый";
            _dbService.AddTagAsync(tag).Wait();
            var readTag = _dbService.GetTagsAsync().Result.ToArray();
            Assert.True(readTag.Length == 1);
            Assert.True(readTag[0] == tag);
        }

        [Fact]
        public void BindTagSuccessfully()
        {
            const string tag = "test string";
            _dbService.AddTagAsync(tag).Wait();
            _dbService.AddMemUnit(new MemoryUnit {Content = "k", CreationDate = DateTime.Now}).Wait();
            var readMem = _dbService.GetMemesAsync().Result.First();
            _dbService.BindTagAsync(readMem.Id, tag);
            var boundTag = _dbService.GetTagsAsync(readMem.Id).Result.First();
            
            Assert.Equal(tag, boundTag);
        }
        
        [Fact]
        public void BindTagNotUnique()
        {
            const string tag = "test string";
            _dbService.AddTagAsync(tag).Wait();
            _dbService.AddMemUnit(new MemoryUnit {Content = "k", CreationDate = DateTime.Now}).Wait();
            var readMem = _dbService.GetMemesAsync().Result.First();
            _dbService.BindTagAsync(readMem.Id, tag);
            var boundTag = _dbService.GetTagsAsync(readMem.Id).Result.First();
            
            Assert.Equal(tag, boundTag);
            Assert.ThrowsAnyAsync<Exception>(() => _dbService.BindTagAsync(readMem.Id, tag));
        }
        
        [Fact]
        public void MultipleTagsBindTagSuccessfully()
        {
            const string tag = "test string";
            const string tag1 = "test string 2";
            _dbService.AddTagAsync(tag).Wait();
            _dbService.AddTagAsync(tag1).Wait();
            
            _dbService.AddMemUnit(new MemoryUnit {Content = "k", CreationDate = DateTime.Now}).Wait();
            _dbService.AddMemUnit(new MemoryUnit {Content = "k", CreationDate = DateTime.Now}).Wait();
            var readMemes = _dbService.GetMemesAsync().Result.ToArray();
            
            _dbService.BindTagAsync(readMemes[0].Id, tag);
            _dbService.BindTagAsync(readMemes[1].Id, tag1);
            var boundTags = _dbService.GetTagsAsync(readMemes[0].Id).Result.ToArray();
            var boundTags1 = _dbService.GetTagsAsync(readMemes[1].Id).Result.ToArray();
            
            Assert.True(boundTags.Length == 1 && boundTags1.Length == 1);
            Assert.Equal(boundTags[0], tag);
            Assert.Equal(boundTags1[0], tag1);
        }

        [Fact]
        public void AddingTagTwiceException()
        {
            const string tag = "tagForAddingTwice";
            _dbService.AddTagAsync(tag).Wait();
            var gotTags = _dbService.GetTagsAsync().Result;
            Assert.True(gotTags.Count() == 1);
            Assert.ThrowsAnyAsync<Exception>(async () => await _dbService.AddTagAsync(tag));
        }
        
        [Fact]
        public void AddingAssociationTagTwiceException()
        {
            const string tag = "tagForAddingTwice";
            _dbService.AddAssociationTagAsync(tag).Wait();
            var gotTags = _dbService.GetAssociationTagsAsync().Result;
            Assert.True(gotTags.Count() == 1);
            Assert.ThrowsAnyAsync<Exception>(async () => await _dbService.AddAssociationTagAsync(tag));
        }

        [Fact]
        public void ReadingAllMannedUnits()
        {
            const int unitsNumber = 20;
            var registeredUnits = _dbService.GetFullyGenerated(unitsNumber, 5).ToArray();
            Assert.True(registeredUnits.Length == unitsNumber);
            for (var i = 1; i < unitsNumber; i++)
            {
                var calculatedCurrent = registeredUnits[i - 1]
                    .Associations.OrderByDescending(a => a.Apex2.Id)
                    .First()
                    .Apex2;
                Assert.Equal(calculatedCurrent, registeredUnits[i]);
            }
        }
        
        public void Dispose()
        {
            // ReSharper disable once ConstantConditionalAccessQualifier
            _dbService?.Dispose();
            var tempDbPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName("dbSource1.db"));
            File.Delete(tempDbPath);
            GC.SuppressFinalize(this);
        }
    }
}