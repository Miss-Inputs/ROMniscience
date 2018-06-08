# ROMniscience
Lister for ROM images and associated header information, written in C# (targeting both .NET and Mono)

## What does it do?
ROMniscience is a thing that gives you all the information secretly embedded in your ROM files and discs. A lot of game consoles use that information so they know how to execute that game correctly, display information to the user when starting up, or maybe just because the manufacturer of that console likes to have it there for their weird licensing/approval purposes. But hey, why question why the info's there? It is there, and it's mildly interesting. So why not? Also, if you have the datfiles from preservation communities such as No-Intro or Redump, it'll verify your files against those to tell you if they're correct or not.

In future, it'll give you the ability to browse filesystems contained inside that ROM, and have a few little tools to manage your ROM collection as well. But not yet.

## But that's vaguely like this thing that already exists! Therefore this is a waste of time! Bah!
Well, first of all, I don't believe in this idea that you should not be allowed to make software that does similar things to another piece of software under the pretense that it's "wasteful". The phrase "reinventing the wheel" sucks. Maybe the only wheels available don't suit my needs exactly. Maybe I want to know how the process of making a wheel works. Maybe there's a monopoly on wheels and you can only buy them under certain conditions. Maybe I just want to make a wheel, y'know?  

Anyway, I didn't know of a lot of these tools existed at the time. It's only after I got so far into development that it'd be too late that I was like "Oh! There's these things that also list ROM information". And I'm like "cool! How about that", and continued doing stuff. The thing is, disregarding that this is a hobby project for me and I like doing it anyway, I don't know what the aims of all those projects are and whether they have the same goals as me, or if they're even actively maintained, so I see it as still worth me doing this thing.  

## So what are those goals?
Ease of use / general UX stuff, clean-ish code (enough that it can be used as documentation for those kinds of people who want to read code as documentation, I guess), basically not doing stupid weird hacks, and accuracy: Be as sure that something is the correct way to read/parse/etc something as it is possible to be.  

Sometimes I just make it up as I go along, but I force myself to stick to those three things.

## What systems does it support?
At the time of writing:  

  - APF-MP1000
  - Atari 5200
  - Atari 7800 (but only the community-developed header)
  - Commodore 64 (only some formats)
  - Dreamcast
  - e-Reader (slightly broken, see comments in that source file)
  - Game Boy Advance  
  - Game Boy/Game Boy Color  
  - GameCube
  - Neo Geo Pocket/Neo Geo Pocket Color  
  - NES, FDS  
  - Nintendo 64, 64DD
  - Nintendo DS  
  - Nintendo 3DS  
  - Pokemon Mini  
  - PSP (only PBP files at this point)
  - RCA Studio II
  - Sega Master System/Game Gear  
  - Sega Megadrive/Genesis, and by extension 32X, Mega CD/Sega CD and Sega Pico since they're the same format  
  - Sega Saturn
  - SNES  
  - Vectrex  
  - Virtual Boy
  - Wii, WiiWare, Wii homebrew folders for Homebrew Channel
  - WonderSwan (and therefore Benessee Pocket Challenge v2)
  - Xbox (.xbe only at this point)
  - Xbox 360 (.xex only at this point)

  Anything else can still be listed to give you generic information, such as size, compression ratio, CRC32/MD5/SHA1, matches of those checksums against datfiles, and such. I've pretty much added every single system possible to get generic information like that. Yeah, every single system possible. All of them. You heard me. Every single one. Try me. I dare you. I have no life. I go out of my way to research obscure things for fun. You can't defeat me. I'm immortal.  

  You can also check in ROMniscience/Handlers, in case I forgot something on that list.


## Will it support <system>?
Possibly! If you have documentation for a system that's only a stub handler, please do make me aware of it somehow, becuase then I'll probably be able to implement it. Some systems just kinda don't have documentation, though, because they're too obscure and nobody cares. Also, some systems don't _have_ any kind of information to extract, which sucks. Also, it helps if I have dumps of that system, and sometimes I kinda don't. (I'd rather not unless I can find a way to dump real media myself, or there's homebrew which actually wants me to download it. You know what I mean? I'm a nice person and I don't like to pirate stuff.)

## Howmst do I compile?
This uses the rather nifty library [SharpCompress](https://github.com/adamhathcock/sharpcompress) so you'll probably need that to build, though if I understand Visual Studio correctly, and I probably don't, you can right click the solution and do "Restore NuGet Packages" and it should all work. For end users, just make sure SharpCompress.dll is somewhere where I can see it, like in the same folder as ROMniscience.exe. That's why I put it there in each release.  

Anyway, it'll build with Visual Studio 2017, and msbuild from Mono 5. Maybe with older C# compilers. Probably not. The version of xbuild present in Ubuntu 17.10's default respositories doesn't work, I know that much. 

## Can I help the project?
If you really want to... well, for developers, I haven't thought about a coding standards guide or stuff like that yet, so that'll happen later.  

But if you're just scanning your ROMs and you see anything that says "Unknown"? Tell me about that value and what ROM it appears in and what's special about it. There may be mysterious secrets laying inside. Some value that corresponds to something that I didn't know about. That's one thing that comes to mind.  

## Hey! I get an exception with this compressed DVD file saying I'm out of memory.
Oh yeah. Should have mentioned, you can't really use .zip or .7z etc to compress really big files like DVD images. It's because I have to uncompress the whole file to memory, which is because of a limitation in SharpCompress where archive streams aren't seekable, and as I understand it that's because of a limitation in the algorithm used by those formats meaning they can never be seekable, no matter how clever of a programmer you may be. Which sucks, I guess. So the bottom line is that you can't really zip any super huge files. Yeah, I know, hard drive space isn't free.  


## Why's it called ROMniscience? That name sucks.
Well, because it's like omniscience, which is knowing all the things, and it's like ROM, and it knows all the things about your ROMs (in theory), so ROM + omniscience = ROMniscience and... look, naming stuff is hard, okay?  

## Why does [GUI thing] suck?  
WinForms.  

## Why do you swear so much in your commit messages and comments?
I find it therapeutic.

## Nobody is actually asking these questions! You're just padding out your readme whilst falsifying the impression that anyone actually asks questions about this.  
Dang.  

## I have some other question that isn't covered here!
Well, ask away. I'm not an elitist, so if something confuses you and you'd prefer it not to, that's fine. It's just that I can't predict every single thing that people who aren't me don't know about this, because of course this is a thing I do know, because I made it, so I don't have a perspective of people who don't know it.  

Unless it's like "what's a ROM?" or something, not because I think you're a terrible person for asking that, it's just that I don't know how to answer it.

So I guess that'll do for a readme? I should go to bed or something.
