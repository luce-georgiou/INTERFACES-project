/**
* Name: sendGeometriesToUnity
* Author: Patrick Taillandier
* Description: A simple model allow to send geometries to Unity. To be used with the "Load geometries from GAMA"
* Tags: gis, shapefile, unity, geometry, 
*/
model sendGeometriesToUnity

import "../models/NBSS_AM_v2.1_forVR.gaml"


global {
	unity_property up_NBSS;
	unity_property up_default;
	unity_property up_road;
	unity_property up_building;
	unity_property up_park;
	unity_property up_lawn;
	
	
	init {
 		// Initialization of NBSS and all its components :
		create NBSS number: 8 {
			shape <- list_of_geoms[index];
			location <- list_of_locs[index];
		}
		create lawn {
			geometry lawn_geom <- rectangle(260, 150) at_location({115, -65});
		    loop i from: 0 to: 7 {
		        geometry nbs <- list_of_geoms[i] at_location list_of_locs[i];
		        lawn_geom <- lawn_geom - nbs;
		    }
		    shape <- lawn_geom;
		}
		create road number: 2 {
			shape <- [rectangle(260, 7), rectangle(7, 150)][index];
			location <- [{one_of(lawn).location.x, one_of(lawn).location.y + 65}, {one_of(lawn).location.x - 15, one_of(lawn).location.y}][index]; 
		}
		create building number: 7 {
			shape <- [rectangle(11, 25), //1
				rectangle(11, 35), //2
				rectangle(18, 12), //3
				rectangle(20, 22), //4
				rectangle(28, 11), //5
				rectangle(28, 11), //6
				rectangle(28, 11) //7
			][index];
			location <- [
				{last(road).location.x - 20, list_of_locs[7].y}, {last(road).location.x - 20, (list_of_locs[6].y + list_of_locs[5].y)/2}, 
				{last(road).location.x - 22, list_of_locs[4].y},
				{last(road).location.x + 16, list_of_locs[7].y}, {last(road).location.x + 20, list_of_locs[6].y}, 
				{last(road).location.x + 20, list_of_locs[5].y}, {last(road).location.x + 20, list_of_locs[4].y}
			][index];
		}
		create park number: 2 {
			shape <- [rectangle(45, 23), // Pole petite enfance
				rectangle(130, 23) // Parc Elie Wiesel
			][index];
			location <- [{list_of_locs[1].x - 7, list_of_locs[1].y - 18}, // Pole petite enfance
				{(list_of_locs[2].x + list_of_locs[3].x)/2 - 4, list_of_locs[1].y - 18} // Parc Elie Wiesel
			][index];
		}
	}
 	

}



//Species that will make the link between GAMA and Unity. It has to inherit from the built-in species asbtract_unity_linker
species unity_linker parent: abstract_unity_linker {
	//name of the species used to represent a Unity player
	string player_species <- string(unity_player);

	//in this model, no information will be automatically sent to the Player at every step, so we set do_info_world to false
	bool do_send_world <- false;
	
	
	//initial location of the player
	list<point> init_locations <- [world.location];
	
	
	init {
		//define the unity properties
		do define_properties;
		do add_background_geometries(NBSS, up_NBSS);
		do add_background_geometries(road, up_road);
		do add_background_geometries(building, up_building);
		do add_background_geometries(park, up_park);
		do add_background_geometries(lawn, up_lawn);
	}
	
	
	//action that defines the different unity properties
	action define_properties {
		unity_aspect default_aspect <- geometry_aspect(1.0,#green,precision);
		up_default <- geometry_properties("default","",default_aspect,#no_interaction,false);
		unity_properties << up_default;

		unity_aspect NBSS_aspect <- geometry_aspect(0.15,#gray,precision);
		up_NBSS <- geometry_properties("NBSS","NBSS",NBSS_aspect,#no_interaction,false);
		unity_properties << up_NBSS;
		
		unity_aspect lawn_aspect <- geometry_aspect(0.1,#green,precision);
		up_lawn <- geometry_properties("lawn","lawn",lawn_aspect,#no_interaction,false);
		unity_properties << up_lawn;
		
		unity_aspect road_aspect <- geometry_aspect(0.2,#gray,precision);
		up_road <- geometry_properties("road","",road_aspect,#no_interaction,false);
		unity_properties << up_road;
		
		unity_aspect building_aspect <- geometry_aspect(10,#gray,precision);
		up_building <- geometry_properties("building","",building_aspect,#no_interaction,false);
		unity_properties << up_building;
		
		unity_aspect park_aspect <- geometry_aspect(0.2,#darkgreen,precision);
		up_park <- geometry_properties("park","",park_aspect,#no_interaction,false);
		unity_properties << up_park;
	}
}

//species used to represent an unity player, with the default attributes. It has to inherit from the built-in species asbtract_unity_player
species unity_player parent: abstract_unity_player {
	//size of the player in GAMA
	float player_size <- 1.0;

	//color of the player in GAMA
	rgb color <- #red ;
	
	//vision cone distance in GAMA
	float cone_distance <- 10.0 * player_size;
	
	//vision cone amplitude in GAMA
	float cone_amplitude <- 90.0;

	//rotation to apply from the heading of Unity to GAMA
	float player_rotation <- 90.0;
	
	//display the player
	bool to_display <- true;
	
	
	//default aspect to display the player as a circle with its cone of vision
	aspect default {
		if to_display {
			if selected {
				 draw circle(player_size) at: location + {0, 0, 4.9} color: rgb(#blue, 0.5);
			}
			draw circle(player_size/2.0) at: location + {0, 0, 5} color: color ;
			draw player_perception_cone() color: rgb(color, 0.5);
		}
	}
}


experiment main type: gui {
	output {
		display map {
			
		}
	}
}

experiment SendGeometriesToUnity parent:main autorun: false type: unity {
	//minimal time between two simulation step
	float minimum_cycle_duration <- 0.05;

	//name of the species used for the unity_linker
	string unity_linker_species <- string(unity_linker);
	
	//allow to hide the "map" display and to only display the displayVR display 
	list<string> displays_to_hide <- ["map"];
	


	//action called by the middleware when a player connects to the simulation
	action create_player(string id) {
		ask unity_linker {
			do create_player(id);
		}
	}

	//action called by the middleware when a plyer is remove from the simulation
	action remove_player(string id_input) {
		if (not empty(unity_player)) {
			ask first(unity_player where (each.name = id_input)) {
				do die;
			}
		}
	}
	
	//variable used to avoid to move too fast the player agent
	float t_ref;

		 
	output { 
		//In addition to the layers in the map display, display the unity_player and let the possibility to the user to move players by clicking on it.
		display displayVR parent: map  {
			species unity_player;
			event #mouse_down  {
				float t <- gama.machine_time;
				if (t - t_ref) > 500 {
					ask unity_linker {
						move_player_event <- true;
					}
					t_ref <- t;
				}
				
			}
		}
		
	} 
}