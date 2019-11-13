[![Build status](https://ci.appveyor.com/api/projects/status/35gwill04wqfo53y/branch/master?svg=true)](https://ci.appveyor.com/project/MakArs/reportserver/branch/master)

# ReportServer
ETL solution(windows service) for scheduled reports compilation and sending

**Technology stack**: [Topshelf](https://github.com/Topshelf/Topshelf), [NancyFX](http://nancyfx.org), [Autofac](https://autofac.org), [FastSql](https://github.com/gerakul/FastSql), [Monik](https://github.com/Totopolis/monik), [NCrontab](https://github.com/atifaziz/NCrontab)

**Input data formats**
* SQL Database
* Excel file
* CSV file
* SFTP server(file transmition)

**Output report formats**
* E-mail(Excel/Json attachements, message body as HTML table)
* Telegram message
* Database(raw data transmition into table/compressed reports)
* FTP/SFTP server(file transmition, CSV,Excel,Json)

# Configuration options
* Via [Consul](https://github.com/MakArs/ReportServer/blob/master/ReportService/ConsulSettings.json)
* Via [App.Config](https://github.com/MakArs/ReportServer/blob/master/ReportService/App.config) 

# Desktop client
* [ReportServer.Desktop](https://github.com/MakArs/ReportServer.Desktop)

# License
* [MIT](https://github.com/MakArs/ReportServer.Desktop/blob/master/LICENSE)
