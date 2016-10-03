/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.97
 */

using System.Reflection;
using System.Runtime.InteropServices;
using System;
using System.Resources;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("PerthFurStats Chat Command Bot")]
[assembly: AssemblyDescription("A Bot for Telegram")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyCompany("Nytewolf Creations")]
[assembly: AssemblyProduct("PerthFurStats Chat Command Bot")]
[assembly: AssemblyCopyright("Copyright © 2016 Nytewolf Creations")]
[assembly: AssemblyTrademark("Telegram is a cloud-based mobile and desktop messaging app with a focus on security and speed.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("4da459dc-df9d-455b-8f25-08e03b71b554")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.97")]
[assembly: AssemblyFileVersion("1.0.0.97")]
[assembly: AssemblyInformationalVersion("1.0.0.97")]
[assembly: NeutralResourcesLanguageAttribute("en-AU")]

//[assembly: PermissionSetAttribute(SecurityAction.RequestMinimum, Name = "FullTrust")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("nwtelegrambot.snk")]
[assembly: AssemblyKeyName("")]
#endif