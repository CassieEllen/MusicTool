Music tool to search different locations containing .mp3 files, and merge them
into a single directory. 

I used nhibernate to access MySql and everything is configured for MySql. 
There were problems with using nhibernate and Sqlite on linux which would
have been my preferred database. Still, it should be possible to use any
database that nhibernate supports. 

The hibernate configuration file is found in Model/hibernate.cfg.xml. The
current connection string requires there be a MySql server running locally
with a database named musictool, with user musictool having a password of
mtpasswd.

Extracted from hibernate.cfg.xml
 <property name="connection.connection_string">Server=localhost;Database=musictool;Uid=musictool;Pwd=mtpasswd;</property>

 Changing to a different database would require changing hibernate.cfg.xml.

 > MusicTool.exe -h
 MusicTool.exe 1.0.6124.13614
 usage: MusicMerge.exe [options] [basedir]...
  -l, --list=VALUE           directory list
  -h, --help                 show this message and exit
  -m, --merge=VALUE          the name of the merge directory
  -v, --verbose              increase message verbosity.
  -f, --fix=VALUE            fix
  -r, --reset                reset tables

-l sets the name of the music list file. 
-h produces the short usage message shown above.
-m sets the name of the merge directory. 
-v sets verbose mode. This is useful to see what is happening
   An alternative is to tail -F MusicTool.log which contains
   much more information.
-f "disk"
-r Resets the database, clearing all previous entries. 

Example Usage
 MusicTool.exe -l music.lst -m /home/cenicol/Music 

I have multiple music source directories so I use file music.lst to 
manage a list of them. You can comment lines out by beginning the line
with a hash (#). Blank lines are ignored. Other lines require a full
path to the top level music source directory. 

Example music.lst file
  --- start ---
  #/home/cenicol/Amazon MP3
  #oopsie

  /home/cenicol/Merge
  /home/cenicol/Music
  /home/cenicol/Amazon MP3

  /mnt/cassie/fedora22/home/cenicol/Music/Amazon MP3
  /mnt/cassie/fedora22/home/cenicol/Music/mp3

  /mnt/cassie/ubuntu14.04.lts/home/cenicol/Music/Amazon MP3
  /mnt/share/Amazon MP3

  /mnt/music/Music  
  --- end ---

 When using -m /path/to/merge/dir, It is best to initially start out with a blank
 directoy. That will prevent existing duplicate files from remaining there.
 Currently there is no provision to remove duplicate files in existing directories.

 If you have any questons, I can be found at CassieENicol at gmail.com