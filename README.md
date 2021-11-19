[![build](https://github.com/MakArs/ReportServer/actions/workflows/build.yml/badge.svg)](https://github.com/MakArs/ReportServer/actions/workflows/build.yml)

# ReportServer
ETL solution(windows service) for scheduled reports compilation and sending

**Technology stack**: [Autofac](https://autofac.org), [Dapper.Contrib](https://github.com/StackExchange/Dapper/tree/master/Dapper.Contrib), [Monik](https://github.com/Totopolis/monik), [NCrontab](https://github.com/atifaziz/NCrontab), [protobuf-net](https://github.com/protobuf-net/protobuf-net)

**Input data formats**
* SQL Database
* Excel file
* CSV file
* SFTP server(file transfer)

**Output report formats**
* E-mail(Excel/Json attachements, message body as HTML table)
* Telegram message
* Database(raw data transfer into table/compressed reports)
* FTP/SFTP server(file transfer, CSV,Excel,Json)

# Configuration options
* Via [Consul](https://github.com/MakArs/ReportServer/blob/master/ReportService.Api/ConsulSettings.json) (needs to have working Consul instance with [AppService](https://github.com/MakArs/ReportServer/blob/master/ReportService.Api/appsettings.json) file configured)
or
* Via [appsettings](https://github.com/MakArs/ReportServer/blob/master/ReportService.Api/appsettings.json) 

# Desktop client
* [ReportServer.Desktop](https://github.com/MakArs/ReportServer.Desktop)

# License
* [MIT](https://github.com/MakArs/ReportServer.Desktop/blob/master/LICENSE)
