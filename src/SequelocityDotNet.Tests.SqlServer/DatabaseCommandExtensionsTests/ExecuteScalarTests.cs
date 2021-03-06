using System.Data;
using NUnit.Framework;

namespace SequelocityDotNet.Tests.SqlServer.DatabaseCommandExtensionsTests
{
    [TestFixture]
    public class ExecuteScalarTests
    {
        [Test]
        public void Should_Return_The_First_Column_Of_The_First_Row_In_The_Result_Set()
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

SELECT  SuperHeroId, /* This should be the only value returned from ExecuteScalar */
        SuperHeroName
FROM    #SuperHero;
";

            // Act
            var superHeroId = Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( sql )
                .ExecuteScalar()
                .ToLong();

            // Assert
            Assert.That( superHeroId == 1 );
        }

        [Test]
        [Description( "This tests the generic version of the ExecuteScaler method." )]
        public void Should_Return_The_First_Column_Of_The_First_Row_In_The_Result_Set_And_Convert_It_To_The_Type_Specified()
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

SELECT  SuperHeroId, /* This should be the only value returned from ExecuteScalar */
        SuperHeroName
FROM    #SuperHero;
";

            // Act
            var superHeroId = Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( sql )
                .ExecuteScalar<long>(); // Generic version of the ExecuteScalar method

            // Assert
            Assert.That( superHeroId == 1 );
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

SELECT  SuperHeroId, /* This should be the only value returned from ExecuteScalar */
        SuperHeroName
FROM    #SuperHero;
";
            var databaseCommand = Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( sql );

            // Act
            databaseCommand.ExecuteScalar();

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

SELECT  SuperHeroId, /* This should be the only value returned from ExecuteScalar */
        SuperHeroName
FROM    #SuperHero;
";
            var databaseCommand = Sequelocity.GetDatabaseCommand( "SqlServer" )
                .SetCommandText( sql );

            // Act
            databaseCommand.ExecuteScalar( true );

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
                .SetCommandText( "SELECT 1" )
                .ExecuteScalar();

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
                .SetCommandText( "SELECT 1" )
                .ExecuteScalar();

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
                .ExecuteScalar();

            // Assert
            Assert.Throws<System.Data.SqlClient.SqlException>( action );
            Assert.IsTrue( wasUnhandledExceptionEventHandlerCalled );
        }
    }
}