ShipIt Inventory Management
===========================

Copyright 2010.

# Setup Instructions

## Running the application

### With Visual Studio

To run the app via Visual Studio:

* Open the `ShipIt.sln` solution by going to `File` -> `Open` -> `Project/Solution`

### With Jetbrains Rider

To run the app in Rider:

* Set up:
* Open the `ShipIt.sln` solution by going to `File` -> `Open` -> `Open Solution or Project`
* Ensure MSTest and MSBuild are installed. MSBuild web development package must be installed.
* Set Rider to target your MSTest installation in File | Settings | Build, Execution, Deployment | Unit Testing | MSTest.
* Set Rider to target your MSBuild installation in File | Settings | Build, Execution, Deployment | Toolset and Build.

### Database:
 
* Install PostgreSQL and set up a database server. 
* Create 2 databases, one called ShipIt and the other called ShipItTest.
* Download a database dump, and run 
```
psql -h localhost -p <dbServerPort> -U <username> -d ShipIt  -f <pathToDbDump>
psql -h localhost -p <dbServerPort> -U <username> -d ShipItTest  -f <pathToDbDump>
```
    (PostgreSQL default server port is 5432 - this can be changed by setting the port value in `<InstallationDirectory>/<versionNumber>/data/postgresql.conf`)
* Add a connections.config to both the ShipIt and ShipItTest projects, adding a connection string to each e.g.
```
<connectionStrings>
  <add name="MyPostgres" providerName="System.Data.SqlClient" connectionString="Server=127.0.0.1;Port=<dbPort>;Database=<dbName>;User Id=<username>; Password=<password>;" />
</connectionStrings>
```

### Deploy to Production

ShipIt is deployed on AWS Elastic Beanstalk with a postgres DB.
To update a running [AWS Elastic Beanstalk](https://aws.amazon.com/elasticbeanstalk/) instance:

* Install [AWS Toolkit for Visual Studio](https://aws.amazon.com/visualstudio/)
* Open the Warehouses-CSharp project in Visual Studio and add your AWS credentials to the AWS Toolkit
* Right click on the ShipIt project and select Publish to AWS
* Select the region your prod environment is running on and redeploy to that environment

To check the logs:  From the AWS console, go to `Services` -> `Elastic Beanstalk`, and
choose your instance from the dashboard.   Click `Logs` on the left, then `Request Logs`.

In the unlikely event that you need to change any of the injected configuration, for
example the database connection string or password, then these are available under
`Configuration` -> `Software`.

Information on the CPU utilisation, and network utilisation is available under `Monitoring`,
it may also be interesting to look at the utilisation or logs of the PostgreSQL database instance
which backs this application.  These are available under `Services` -> `RDS` -> `Databases`
-> `shipit`.

## Unit Tests

Run the tests in Visual Studio by right clicking on the `ShipItTest` project and
choosing "Run Tests".
