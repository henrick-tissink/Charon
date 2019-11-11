A simple database migration tool.

* Add migration scripts into a `MigrationScripts` directory within the executing assembly (the executing project) and call `Charon.Ferry("with the appropriate connection string")`.

* Scripts will be run in the order they are kept within the `MigrationScripts` directory. It is therefore recommended to adopt a naming approach such as `script00001 - the change to migrate.sql`, to ensure the scripts are run in the desired order, from oldest to newest.

![alt text](https://travis-ci.com/henrick-tissink/Charon.svg?branch=master)
