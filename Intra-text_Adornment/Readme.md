# Intra-Text Adornment Sample
Provide colored adornments to hexadecimal color values in Visual Studio.

* Technologies: Visual Studio 2015 SDK
* Topics: Visual Studio Editor, VSX

**Description**

This sample Visual Studio 2015 extension uses intra-text adornments to replace
six digit hexadecimal values with color swatches. These adornments are shown
between text characters, as opposed to behind or in front. Intra-text
adornments may optionally replace text. The value of the hexadecimal number
corresponds to the color of the swatch.

  * Red = 0xff0000 
  * Green = 0x00ff00 
  * Blue = 0x0000ff 
  
![image](C%23/Example.IntraTextAdornment.png)

**Requirements**

[ Visual Studio 2015 ](https://www.visualstudio.com/products/visual-studio-community-vs?wt.mc_id=o~display~github~vssdk)



**Get all samples**

Clone the repo ([How to](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository#Cloning-an-Existing-Repository)):

`git clone https://github.com/Microsoft/VSSDK-Extensibility-Samples.git`

**Run the sample**

  1. To run the sample, hit **F5** or choose the **Debug &gt; Start Debugging** menu command. A new experimental instance of Visual Studio will launch. 
  2. Once loaded, open any file in the text editor. 
  3. Type a six-digit hexidecimal number. 
  4. An adornment is displayed that displays the color value of the hexadecimal number 



If you want the color swatch to appear next to the hexadecimal number instead
of replace it, comment out the line `#define HIDING_TEXT` in
`ColorAdornmentTagger.cs`.



**Related topics**

  * [ Editor Documentation ](https://msdn.microsoft.com/en-us/library/dd885242(v=vs.140).aspx)
  * [ Visual Studio SDK Documentation ](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.140).aspx)



