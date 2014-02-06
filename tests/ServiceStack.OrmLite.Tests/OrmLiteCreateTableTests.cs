using System;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    using System.Reflection;
    using System.Runtime.InteropServices;

    using ServiceStack.DataAnnotations;

    [TestFixture]
	public class OrmLiteCreateTableTests 
		: OrmLiteTestBase
	{
		[Test]
		public void Does_table_Exists()
		{
			using (var db = OpenDbConnection())
			{
				db.DropTable<ModelWithIdOnly>();

				Assert.That(
                    db.TableExists(typeof(ModelWithIdOnly).Name),
					Is.False);
				
				db.CreateTable<ModelWithIdOnly>(true);

				Assert.That(
                    db.TableExists(typeof(ModelWithIdOnly).Name),
					Is.True);
			}
		}

		[Test]
		public void Can_create_ModelWithIdOnly_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdOnly>(true);
			}
		}

		[Test]
		public void Can_create_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
			}
		}

		[Test]
		public void Can_create_ModelWithLongIdAndStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithLongIdAndStringFields>(true);
			}
		}

		[Test]
		public void Can_create_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);
			}
		}

		[Test]
		public void Can_preserve_ModelWithIdOnly_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdOnly>(true);

				db.Insert(new ModelWithIdOnly(1));
				db.Insert(new ModelWithIdOnly(2));

				db.CreateTable<ModelWithIdOnly>(false);

				var rows = db.Select<ModelWithIdOnly>();

				Assert.That(rows, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void Can_preserve_ModelWithIdAndName_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);

				db.Insert(new ModelWithIdAndName(1));
				db.Insert(new ModelWithIdAndName(2));

				db.CreateTable<ModelWithIdAndName>(false);

				var rows = db.Select<ModelWithIdAndName>();

				Assert.That(rows, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void Can_overwrite_ModelWithIdOnly_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdOnly>(true);

				db.Insert(new ModelWithIdOnly(1));
				db.Insert(new ModelWithIdOnly(2));

				db.CreateTable<ModelWithIdOnly>(true);

				var rows = db.Select<ModelWithIdOnly>();

				Assert.That(rows, Has.Count.EqualTo(0));
			}
		}

		[Test]
		public void Can_create_multiple_tables()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTables(true, typeof(ModelWithIdOnly), typeof(ModelWithIdAndName));

				db.Insert(new ModelWithIdOnly(1));
				db.Insert(new ModelWithIdOnly(2));

				db.Insert(new ModelWithIdAndName(1));
				db.Insert(new ModelWithIdAndName(2));

				var rows1 = db.Select<ModelWithIdOnly>();
				var rows2 = db.Select<ModelWithIdOnly>();

				Assert.That(rows1, Has.Count.EqualTo(2));
				Assert.That(rows2, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void Can_create_ModelWithIdAndName_table_with_specified_DefaultStringLength()
		{
			OrmLiteConfig.DialectProvider.DefaultStringLength = 255;
			var createTableSql =  OrmLiteConfig.DialectProvider.ToCreateTableStatement(typeof(ModelWithIdAndName));

			Console.WriteLine("createTableSql: " + createTableSql);
			Assert.That(createTableSql.Contains("VARCHAR(255)"), Is.True);
		}

        public class ModelWithGuid
        {
            public long Id { get; set; }
            public Guid Guid { get; set; }
        }

        [Test]
        public void Can_handle_table_with_Guid()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithGuid>();

                db.GetLastSql().Print();
    
                var models = new[] {
                    new ModelWithGuid { Id = 1, Guid = Guid.NewGuid() }, 
                    new ModelWithGuid { Id = 2, Guid = Guid.NewGuid() }
                };

                db.SaveAll(models);

                var newModel = db.SingleById<ModelWithGuid>(models[0].Id);

                Assert.That(newModel.Guid, Is.EqualTo(models[0].Guid));

                newModel = db.Single<ModelWithGuid>(q => q.Guid == models[0].Guid);

                Assert.That(newModel.Guid, Is.EqualTo(models[0].Guid));
            }
        }

        public class ModelWithOddIds
        {
            [Index(false)]
            public long Id { get; set; }

            [PrimaryKey]
            public Guid Guid { get; set; }
        }

        public class ModelThatReferencesModelWithOddIds
        {
            [Index(false)]
            public long Id { get; set; }

            [PrimaryKey]
            public Guid Guid { get; set; }

            [References(typeof(ModelWithOddIds))]
            public Guid ModelWithOddIdsId { get; set; }
        }

        [Test]
        public void Can_handle_table_with_non_conventional_id()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithOddIds>();

                db.GetLastSql().Print();
            }
        }

        [Test]
        public void Can_handle_references_with_non_conventional_ids()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithOddIds>();
                db.DropAndCreateTable<ModelThatReferencesModelWithOddIds>();

                db.GetLastSql().Print();

                db.Insert(new ModelWithOddIds() { Id = 1, Guid = new Guid("CDE11899-9CAF-4885-A881-BDF36AD54525") });
                db.Insert(new ModelThatReferencesModelWithOddIds() { Id = 1, Guid = Guid.NewGuid(), ModelWithOddIdsId = new Guid("CDE11899-9CAF-4885-A881-BDF36AD54525") });

                db.GetLastSql().Print();
            }
        }

        public class ModelWithNonPrimaryKeyForeignKeyReference
        {
            [PrimaryKey]
            public Guid Guid { get; set; }

            [PropertyReference(typeof(ModelWithOddIds), "Id")]
            public int ModelWithOddIdsId { get; set; }
        }

        [Test]
        public void Can_have_reference_to_non_primary_key_field()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithOddIds>();
                db.DropAndCreateTable<ModelWithNonPrimaryKeyForeignKeyReference>();

                db.GetLastSql().Print();

                db.Insert(new ModelWithOddIds() { Id = 1, Guid = new Guid("CDE11899-9CAF-4885-A881-BDF36AD54525") });
                db.GetLastSql().Print();

                db.Insert(new ModelWithNonPrimaryKeyForeignKeyReference() { Guid = Guid.NewGuid(), ModelWithOddIdsId = 1 });

                db.GetLastSql().Print();
            }
        }
	}
}