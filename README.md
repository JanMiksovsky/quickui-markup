quickui-markup
==============

This is the *optional* compiler for QuickUI markup.
This feature is not required for QuickUI's normal use. The QuickUI Catalog
(in the quickui-catalog repo) uses this, as does at least one commericial
application, but going forward the standard way to create QuickUI controls is
with plain JavaScript and/or CoffeeScript.

### Grunt.js

The markup compiler can be installed and invoked as a "qb" task in Grunt.js:

```
npm install quickui-markup
```

### Visual Studio

The Tools.sln can be opened and built with Microsoft Visual Studio Express
(which is free). The Setup/Setup.sln project, which builds the installer,
requires the full Visual Studio product.

### Mono
Tested with Mono 2.8.

If you have monotools installed, you can build the tools by issuing the
following command from this directory:

    xbuild /p:Configuration=Release
