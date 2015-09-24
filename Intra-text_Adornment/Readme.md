# Title: Intra-Text Adornment Sample
**Abstract:** Provide colored adornments to hexadecimal color values.

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Editor, VSX
* Last Updated: 05/18/2015

**Description**

This sample Visual Studio 2015 extension uses intra-text adornments to replace
six digit hexadecimal values with color swatches. These adornments are shown
between text characters, as opposed to behind or in front. Intra-text
adornments may optionally replace text. The value of the hexadecimal number
corresponds to the color of the swatch.

  * Red = 0xff0000 
  * Green = 0x00ff00 
  * Blue = 0x0000ff 


**Requirements**

[ Visual Studio 2015 ](http://www.microsoft.com/visualstudio/en-us/try/default.mspx#download)

[ Visual Studio 2015 SDK ](https://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs.aspx)



**Run the sample**

  1. To run the sample, hit **F5or choose the **Debug &gt; Start Debuggingmenu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, open any file in the text editor. 
  3. Type a six-digit hexidecimal number. 
  4. An adornment is displayed that displays the color value of the hexadecimal number 



If you want the color swatch to appear next to the hexadecimal number instead
of replace it, comment out the line**#define HIDING_TEXTin
_ColorAdornmentTagger.cs_.



**Related topics**

  * [ Editor Documentation ](https://msdn.microsoft.com/en-us/library/dd885242(v=vs.140).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



