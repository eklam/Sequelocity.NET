using System.Data;
using NUnit.Framework;

namespace SequelocityDotNet.Tests.SqlServer.DatabaseCommandExtensionsTests
{
    [TestFixture]
    public class ExecuteToDynamicObjectTests
    {
        public class SuperHero
        {
            public long SuperHeroId;
            public string SuperHeroName;
        }

        [Test]
        public void Should_Map_The_Results_Back_To_A_List_Of_Dynamic()
        {
            // Arrange
            const string sql = @"
CREATE TABLE #SuperHero
(
    SuperHeroId     INT             NOT NULL    IDENTITY(1,1)   PRIMARY KEY,
    SuperHeroName	NVARCHAR(120)   NOT NULL
);

INSERT INTO #SuperHero ( SuperHeroName )
VALUES ( 'Superman' );

INSERT INTO #SuperHero ( SuperHeroName )
VALUES ( 'Batman' );

SELECT  SuperHeroId,
        SuperHeroName
FROM    #SuperHero;
";

            // Act
            var superHero = Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( sql )
                .ExecuteToDynamicObject();

            // Assert
            Assert.NotNull( superHero );
            Assert.That( superHero.SuperHeroId == 1 );
            Assert.That( superHero.SuperHeroName == "Superman" );
        }

        [Test]
        public void Should_Null_The_DbCommand_By_Default()
        {
            // Arrange
            const string sql = @"
CREATE TABLE #SuperHero
(
    SuperHeroId     INT             NOT NULL    IDENTITY(1,1)   PRIMARY KEY,
    SuperHeroName	NVARCHAR(120)   NOT NULL
);

INSERT INTO #SuperHero ( SuperHeroName )
VALUES ( 'Superman' );

INSERT INTO #SuperHero ( SuperHeroName )
VALUES ( 'Batman' );

SELECT  SuperHeroId,
        SuperHeroName
FROM    #SuperHero;
";
            var databaseCommand = Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( sql );

            // Act
            databaseCommand.ExecuteToDynamicObject();

            // Assert
            Assert.IsNull( databaseCommand.DbCommand );
        }

        [Test]
        public void Should_Keep_The_Database_Connection_Open_If_keepConnectionOpen_Parameter_Was_True()
        {
            // Arrange
            const string sql = @"
CREATE TABLE #SuperHero
(
    SuperHeroId     INT             NOT NULL    IDENTITY(1,1)   PRIMARY KEY,
    SuperHeroName	NVARCHAR(120)   NOT NULL
);

INSERT INTO #SuperHero ( SuperHeroName )
VALUES ( 'Superman' );

INSERT INTO #SuperHero ( SuperHeroName )
VALUES ( 'Batman' );

SELECT  SuperHeroId,
        SuperHeroName
FROM    #SuperHero;
";
            var databaseCommand = Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( sql );

            // Act
            databaseCommand.ExecuteToDynamicObject( true );

            // Assert
            Assert.That( databaseCommand.DbCommand.Connection.State == ConnectionState.Open );

            // Cleanup
            databaseCommand.Dispose();
        }

        [Test]
        public void Should_Call_The_DatabaseCommandPreExecuteEventHandler()
        {
            // Arrange
            bool wasPreExecuteEventHandlerCalled = false;

            Sequelocity.ConfigurationSettings.EventHandlers.DatabaseCommandPreExecuteEventHandlers.Add( command => wasPreExecuteEventHandlerCalled = true );

            // Act
            Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( "SELECT 1 as SuperHeroId, 'Superman' as SuperHeroName" )
                .ExecuteToDynamicObject();

            // Assert
            Assert.IsTrue( wasPreExecuteEventHandlerCalled );
        }

        [Test]
        public void Should_Call_The_DatabaseCommandPostExecuteEventHandler()
        {
            // Arrange
            bool wasPostExecuteEventHandlerCalled = false;

            Sequelocity.ConfigurationSettings.EventHandlers.DatabaseCommandPostExecuteEventHandlers.Add( command => wasPostExecuteEventHandlerCalled = true );

            // Act
            Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( "SELECT 1 as SuperHeroId, 'Superman' as SuperHeroName" )
                .ExecuteToDynamicObject();

            // Assert
            Assert.IsTrue( wasPostExecuteEventHandlerCalled );
        }

        [Test]
        public void Should_Call_The_DatabaseCommandUnhandledExceptionEventHandler()
        {
            // Arrange
            bool wasUnhandledExceptionEventHandlerCalled = false;

            Sequelocity.ConfigurationSettings.EventHandlers.DatabaseCommandUnhandledExceptionEventHandlers.Add( ( exception, command ) =>
            {
                wasUnhandledExceptionEventHandlerCalled = true;
            } );

            // Act
            TestDelegate action = () => Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( "asdf;lkj" )
                .ExecuteToDynamicObject();

            // Assert
            Assert.Throws<System.Data.SqlClient.SqlException>( action );
            Assert.IsTrue( wasUnhandledExceptionEventHandlerCalled );
        }
    }
}