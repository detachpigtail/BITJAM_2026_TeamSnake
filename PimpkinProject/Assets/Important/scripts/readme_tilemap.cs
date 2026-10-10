


/*Tilemaps are a unity (component?) that allow us to place the tiles/sprites in the game a lot easier.
you can create a tilemap by M2 the hiearchy > 2D > Tilemap rectangle. 
this will add the tilemap to the current scene, you can adjust all the things assosiated with
it such as the grid size through the inspector window.
you can also have multiple tilemaps on a single scene all for different purposes if that
feel better to use.*/

/*TO put sprites and such on the tilemap. you need to create a Tilepallette. 
you can do so: Window > 2D Tile Pallette. It will open a window in which you will need to
create a new pallette. you dont really need to adjust any of the settings in it such as cell
size. since those only affect how the pallette shows the available options and not how they
will appear in game. I've made a Folder in the important folder for all Pallettes so prob
save it there.*/

/* Once you ahve your Pallette you can import the assets/sprites into it by just dragging
the wanted sprite into the pallette. itll ask you to again save it so just save it in the 
pallette folder. afterward you can place your tiles etc. There is also options on the
pallette to choose which tilemap you are painting on if have multiple so remember to check that.
Right now the sprites are still a but broken and i havent fully worked out the numbers and such.*/

/*if the sprites or something look weird like fuzzy come talk to me or check if the filtermode
on the sprite is set to pivot (in the inspector when highligting the sprites) There might
be some other random setting you need to fiddle around w. really i dont know the proper way to
do ts but this kinda works.*/