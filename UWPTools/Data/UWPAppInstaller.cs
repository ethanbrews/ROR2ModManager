/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace UWPTools.Data.UWPAppInstaller
{
	[XmlRoot(ElementName = "MainBundle", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
	public class MainBundle
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName = "Version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName = "Publisher")]
		public string Publisher { get; set; }
		[XmlAttribute(AttributeName = "Uri")]
		public string Uri { get; set; }
	}

	[XmlRoot(ElementName = "Package", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
	public class Package
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName = "Publisher")]
		public string Publisher { get; set; }
		[XmlAttribute(AttributeName = "Version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName = "ProcessorArchitecture")]
		public string ProcessorArchitecture { get; set; }
		[XmlAttribute(AttributeName = "Uri")]
		public string Uri { get; set; }
	}

	[XmlRoot(ElementName = "Dependencies", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
	public class Dependencies
	{
		[XmlElement(ElementName = "Package", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
		public List<Package> Package { get; set; }
	}

	[XmlRoot(ElementName = "OnLaunch", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
	public class OnLaunch
	{
		[XmlAttribute(AttributeName = "HoursBetweenUpdateChecks")]
		public string HoursBetweenUpdateChecks { get; set; }
	}

	[XmlRoot(ElementName = "UpdateSettings", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
	public class UpdateSettings
	{
		[XmlElement(ElementName = "OnLaunch", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
		public OnLaunch OnLaunch { get; set; }
	}

	[XmlRoot(ElementName = "AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
	public class AppInstaller
	{
		[XmlElement(ElementName = "MainBundle", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
		public MainBundle MainBundle { get; set; }
		[XmlElement(ElementName = "Dependencies", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
		public Dependencies Dependencies { get; set; }
		[XmlElement(ElementName = "UpdateSettings", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
		public UpdateSettings UpdateSettings { get; set; }
		[XmlAttribute(AttributeName = "Uri")]
		public string Uri { get; set; }
		[XmlAttribute(AttributeName = "Version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }
	}

}
