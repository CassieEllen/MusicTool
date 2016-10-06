using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using MusicTool.Model.Domain;

namespace MusicTool.Model
{
	public class ConfigureHibernate
	{		
		public ISessionFactory Factory { get; set; }

		public ConfigureHibernate (bool reset)
		{
			Console.WriteLine (typeof(MusicFile).Assembly.ToString ());
			var cfg = new Configuration ();
			cfg.Configure ();
			cfg.AddAssembly (typeof(MusicFile).Assembly);

			Factory = cfg.BuildSessionFactory ();

			if (Factory == null) {
				throw new NullReferenceException ("Hibernate Factory is null");
			}

			// For the following section to work, property hbm2ddl.auto must be commented out.
			//   <!--
			//   <property name="hbm2ddl.auto">update</property>
			//	 -->
			// Uncomment the property hbm2ddl.auto to always update the table automitically.
			// and the following section will not matter.

			if(reset) {
				var schema = new SchemaExport(cfg);
				schema.Execute(false, true, false);
			} else {
				new SchemaUpdate (cfg).Execute (false, true);
			}

			#if SHOW_SCHEMA
			NHibernate.Dialect.Dialect d = new NHibernate.Dialect.MySQLDialect();
			string[] x = cfg.GenerateSchemaCreationScript (d);
			foreach (var s in x) {
			Console.WriteLine (s);
			}
			#endif

			#if CLEAR_ENTRIES
			if (Options.Reset) {
			using (ISession session = factory.OpenSession ())
			using (ITransaction tx = session.BeginTransaction ()) 
			{
			session.CreateQuery("delete MusicFile f").ExecuteUpdate();
			session.CreateQuery("delete RejectFile r").ExecuteUpdate();
			session.CreateQuery("delete IgnoreFile i").ExecuteUpdate();
			tx.Commit ();
			}
			}
			#endif
		}
	}
}

