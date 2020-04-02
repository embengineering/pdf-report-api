# PDF Report API using ASP.NET Core 3.0

Demonstrate how to generate a PDF via an API using a custom controller action attribute to centralize the implementation.

The following [website](https://nugetmusthaves.com/Tag/html-to-pdf) tracks down tools for generating HTML to PDF reports. I selected the top three I have used in the past or are known to me.

I will be focusing on the following tools:

## [Iron PDF Core](https://nugetmusthaves.com/Package/PDF.Core)

**PROS**

* Implementation is relative simple with good documentation
* Do not require any additional installation

**CONS**

## [Select PDF](https://www.nuget.org/packages/Select.HtmlToPdf.NetCore)

**PROS**

* Implementation is relative simple with good documentation
* Do not require any additional installation

**CONS**

* Footer page numbering require a specific C# object and doesn't work from the HTML directly

## [EVO PDF](https://www.nuget.org/packages/EvoPdf.HtmlToPdf.NetCore/)

**PROS**

* Implementation is relative simple with good documentation

**CONS**

* Require a service installation in order to convert HTML to PDF