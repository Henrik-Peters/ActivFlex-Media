# ActivFlex Media

ActivFlex can be used for various media functionalities like managing, displaying or grouping media items.
At the moment this is an early version and just provides some basic features like image browsing.

**Versionstatus Development: 0.1**

## Prerequisites / Tools

The following things were used to create, compile or support the development of ActivFlex.

* Windows OS
* .NET Framework 4.5.2
* Visual Studio 2017
* Blend for Visual Studio 2017
* Inkscape (Icon-Graphics)

## Project structure

#### ActivFlex Core
Contains the main standalone logic of ActivFlex. This module should not have
any dependencies against other libraries from ActivFlex. Other projects should
be treated as "plugins" to the core functions in this module.

#### ActivFlex Media
This project is responsible for the graphical user interface (GUI) of the
application. This project should contain all icons, styles, view models and
most of the WPF markup files (window descriptions, resource dictionaries).

#### ActivFlex Presenter
All logic that is related to presenting media items (like displaying images,
playing music) should be placed in this module.

## Miscellaneous

At the moments all icons and controls are vector-based. This can be very useful in the future to create
an application that is dpi and resolution independent. For a presentation heavily related to media content,
this could be very useful.

Another aspect of the application is the ability to customize the design layout. For example, it may be nice
for the user to change colors, background images, keyboard shortcuts or hide controls that are "unimportant".
Therefore it is good when controls, styles or layouts are built in a way that allows them to be easily changed,
customized or configurated.

Until now all icon or graphic resources have been created using Inkscape. These resources are included in the
project in the XAML format to use them in WPF. Some of them however are just prototypes that never made it into
the application and are of a different file format (SVG for example). All these "prototype resources" may be
uploaded later to the project, when this will be useful.