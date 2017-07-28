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


## Changelog / Version info

For every release you will find a brief description here, describing what has changed or has been added
to the project. This should be a shorter and more readable summary than the actual commit history. Some 
changes may be left out here if they are not necessarily important to the whole project (like small bug fixes).

### ActivFlex Media - Version 0.1

* Images will be presented when passed as startup arguments
* Size and position of images can be changed in presentation mode
* Images can be displayed and exchanged in presentation mode
* Navigation control for the browsing path added
* The thumbnail size can be changed with a zoom control
* Images have thumbnails that keep their aspect ratio
* Browsing in the file system is now possible
* The main navigation contains all logical drives from the filesystem
* GUI allows switching between normal and fullscreen mode
* Custom window layout design and resize logic

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