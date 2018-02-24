# ROMniscience
Lister for ROM images and associated header information  

Note that this readme will probably be rewritten once people actually start using this and I have more of an idea of what information I actually want to convey, and when I regain the ability to communicate effectively.

ROMniscience is a desktop application for listing your ROM files and displaying the header information from them, or you can also drag and drop an individual ROM file and it'll also display the header information from it. I hope that makes sense.  

I guess it's a bit like uCon64, but it's not meant to replace that - I only found out that exists a while after I started development on this. Oh well! It doesn't look like it does the same kind of thing that I want ROMniscience to do, so I'm not wasting my time or anything. ROMniscience also supports Logiqx datfiles for identification of ROMs, so it's a bit like ClrMAMEPro, or perhaps you could see it as completely obsoleting that [ROMRecognizer](https://github.com/Zowayix/rom-recognizer) project I made a while back, which I consider a sort of stepping stone.  

The focus here is on user friendliness (it's not really doing great there, but I try and make things un-confusing where possible), and accuracy - I make a point of ensuring that I'm actually parsing the headers the right way, and I test them against actual ROMs where possible too. Did you know that N64, Megadrive, and SNES actually don't use plain ASCII in their internal names, and it's actually Shift-JIS? It's true? I know because I actually damn checked.

Because of the latter part, the code could be useful as documentation for anyone who needs ROM header documentation. But code readability and maintainability is my third focus I guess.

It runs on .NET because I like C# and it's cool, but also should run on Linux with Mono (albeit it'll be ugly due to WinForms), and I do care about making sure that works. It might run on Mac OS X with Mono too, I don't have access to a Mac OS X installation, so I don't know if it works or not. I hope it does!  Go ahead and let me know if it doesn't, though there might not be anything I can do about it.  

It's useful for when you have some weirdly named ROM file and you're trying to figure out what the heck it is, or you really like emulation and have a large collection of ROMs and want to make a list of what you have (I think that's why I originally started developing this), or maybe you just like to poke around in hidden things and see internal names and whatnot, I don't judge.  

As they always say with anything ROM/emulation-related, don't commit piracy, because that makes me look bad. But in the context of this, I guess it's just... don't download games that aren't released for free just to see what their ROM header looks like? I say "that aren't released for free", because if a game's license permits you to download it from the internet (as with most homebrew), go nuts. Basically, ROMniscience grants you no additional permissions over the licenses of existing software that it happens to interact with. That's not how the MIT license works. That's not how anything works.  

For a list of ROM formats it'll read, look inside the ROMniscience.Handlers namespace. Maybe I should make a thing that makes a human readable thing that lists them all. I don't really want to update this readme every time I add a new handler, but as of the time of writing:  
  - Atari 7800 (but only the community-developed header)
  - Game Boy Advance  
  - Game Boy/Game Boy Color  
  - Neo Geo Pocket/Neo Geo Pocket Color  
  - NES (not FDS for now)  
  - Nintendo 64, 64DD  
  - Nintendo DS  
  - Pokemon Mini  
  - Sega Master System/Game Gear  
  - Sega Megadrive/Genesis, and by extension 32X and Sega Pico since they're the same format  
  - SNES  
  - Vectrex  
  - Virtual Boy  
  - WonderSwan (and therefore Benessee Pocket Challenge v2)  

  
Many other systems have support for listing ROMs and matching against datfiles (No-Intro is the primary reason for me doing this) but not reading the header, including that one obscure system you were about to think of to try and trip me up. You can't defeat me, I'm a huge nerd about this kind of thing and I also have no life.

This uses the rather nifty library [SharpCompress](https://github.com/adamhathcock/sharpcompress) so you'll probably need that to build, though if I understand Visual Studio correctly, and I probably don't, you can right click the solution and do "Restore NuGet Packages" and it should all work. For end users, just make sure SharpCompress.dll is somewhere where I can see it, like in the same folder as ROMniscience.exe. That's why I put it there in each release.  

Anyway, it'll build with Visual Studio 2017, and msbuild from Mono 5. Maybe with older C# compilers. Probably not. The version of xbuild present in Ubuntu 17.10's default respositories doesn't work, I know that much. 

## Why's it called ROMniscience? That name sucks.
Well, because it's like omniscience, which is knowing all the things, and it's like ROM, and it knows all the things about your ROMs (in theory), so ROM + omniscience = ROMniscience and... look, naming stuff is hard, okay?  

## Why does the settings dialog suck?  
WinForms.  

But I'll un-suck-make it later! I swear! Probably.

## Nobody is actually asking these questions! You're just padding out your readme whilst falsifying the impression that anyone actually asks questions about this.  
Dang.  

Uhh that'll do for a readme I guess? Well, if anything's confusing, let me know and maybe I can explain it here once I know what to explain. Really, it's okay! I do believe one of the things with programming is that I can only see things from my own perspective, which is biased because I'm the one developing and designing the program, so of course I know how it works and it all feels obvious to me. But it might be that for everyone else it's hard to use, or you don't know what to do with it, and I wouldn't really know. Unless you tell me.  
