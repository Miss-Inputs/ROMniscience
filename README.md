# ROMniscience
Lister for ROM images and associated header information  

ROMniscience is a desktop application for listing your ROM files and displaying the header information from them, or you can also drag and drop an individual ROM file and it'll also display the header information from it. I hope that makes sense.  

It runs on .NET because I like C# and it's cool, but also should run on Linux with Mono (albeit it'll be ugly due to WinForms), and I do care about making sure that works. It might run on Mac OS X with Mono too, I don't have access to a Mac OS X installation, so I don't know if it works or not. I hope it does!

It's useful for when you have some weirdly named ROM file and you're trying to figure out what the heck it is, or you really like emulation and have a large collection of ROMs and want to make a list of what you have (I think that's why I originally started developing this), or maybe you just like to poke around in hidden things and see internal names and whatnot, I don't judge.  

For a list of ROM formats it'll read, look inside the ROMniscience.Handlers namespace.

Obligatory disclaimer about piracy goes here, because I said the word "ROM".  

Things that are going to happen when I do them:  
  - Read datfiles from No-Intro, Maybe-Intro, Redump, etc. These won't be distributed with it, for three reasons:  
    1) They're updated like every week it seems so I'd either have to keep updating them or just have old files and I don't like that  
    2) Some of these sites might not like their XML files being redistributed, they don't really say but it's best to not risk it  
    3) They're big! Some are over 2MB by themselves, but when you distribute them all together, you have 100MB worth of files for a small program weighing in at 56KB at the time of writing.  
		Anyway, that should make it significantly more useful, because now you can find out what your files actually are.  
  - Read compressed files (zip and 7z are very important, maybe gzip and bzip2 should be possible, fuck RAR)  
  - Improve the functionality to view an individual file, right now it makes a lot of assumptions about filetypes and the actual type  
  - Include a shell script for Linux that launches it with Mono, so I can drag and drop stuff easier, or maybe a .desktop entry  
  - Export to CSV/TSV/XLSX/ODS, something that I can open in LibreOffice or whatever
  - More handlers! I know there's headers for a lot more ROM formats that I can get relatively interesting info out of, just need to find more documentation.  
  - On that note, once I add a generic way to read CD images (since the byte alignment on .bin/.cue and .iso are different), add support for disc based systems. I could leverage the Megadrive handler to read Mega CD discs since the header is actually the same.

I know these can be done because I've ended up doing them in my experiments trying to find a combination of language/GUI toolkit that is easy for me to work with, easy for me to distribute, and easy for users to use. I just felt like putting this on Github before I started doing them.  

Note that on Linux, it won't display any images (like Nintendo DS icons) inside the table, because Mono is weird and kept screwing up the rest of the cells when I did that. I guess I should raise that as an issue with myself.  

## Why's it called ROMniscience? That name sucks.
Well, because it's like omniscience, which is knowing all the things, and it's like ROM, and it knows all the things about your ROMs (in theory), so ROM + omniscience = ROMniscience and... look, naming stuff is hard, okay?  

## Why does the settings dialog suck?
WinForms.  

Uhh that'll do for a readme I guess? Well, if anything's confusing, let me know and maybe I can explain it here once I know what to explain.
