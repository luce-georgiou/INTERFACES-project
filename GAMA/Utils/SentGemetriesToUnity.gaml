/**
* Name: sendGeometriesToUnity
* Author: Patrick Taillandier
* Description: A simple model allow to send geometries to Unity. To be used with the "Load geometries from GAMA"
* Tags: gis, shapefile, unity, geometry, 
*/
model sendGeometriesToUnity

import "../models/NBSS_AM_v2.1_forVR.gaml"


global {
	unity_property up_filter_media;
	unity_property up_rain;
	//unity_property up_programmed_maintenance;
		//unity_property up_unmanaged_flow;
		//unity_property up_managed_flow;
	unity_property up_vegetation_cover;
	//unity_property up_vegetal_component;
	unity_property up_trees;
	unity_property up_inlet;
	unity_property up_engineered_component;
	unity_property up_natural_environment;
	unity_property up_NBSS;
	unity_property up_default;
	//unity_property up_component;
	//unity_property up_ext_time_failure;
	//unity_property up_ext_metric_failure;
	//unity_property up_failure_event;
	//unity_property up_output_flow;
	//unity_property up_rtf_maintenance;
	unity_property up_outlet;
	unity_property up_urban_environment;
	unity_property up_ponding_area;
	
	//mes agents ajoutés
	unity_property up_shrubs_plants;
	unity_property up_grass;
//	unity_property up_grass2;
//	unity_property up_grass3;
	unity_property up_flower;
//	unity_property up_flower2;
	unity_property up_vegetal_waste;
	unity_property up_trash;
	unity_property up_weeds;
	unity_property up_gravel;
	unity_property up_microorganisms;
	unity_property up_swale;
	unity_property up_lawn;
	unity_property up_lawn_mower;
	
	
	init {
 		// Initialization of NBSS and all its components :
		list<geometry> list_of_geoms <- [rectangle(25.1, 2.65), rectangle(14.7, 2.65), rectangle(16.7, 2.65), rectangle(24.5, 2.65), 
				rectangle(24.5, 2.4), rectangle(26.1, 2.4), rectangle(61.5, 2.93), rectangle(63.2, 2.93)
			];
			create NBSS number: 8 {
			
			shape <- list_of_geoms[index];
			
			create inlet {
				shape <- circle(1);
				location <- {myself.location.x - 13.0, myself.location.y, 0};
			}
			create ponding_area  {
				shape <- rectangle(30, 8);
				location <- {myself.location.x, myself.location.y, 0}; //z+1.75
			}
			create grass number: rnd(0, 100) {
				//if free_space != nil and !empty(free_space) {
					shape <- circle(0.5);
					location <- any_location_in(free_space);
					//free_space <- free_space - (shape + 0.1); 
			}
			create shrubs_plants number: rnd(0,7) {
					shape <- circle(1.5);
					location <- any_location_in(free_space);
			}
			create trees number: rnd(0,5) {
				shape <- circle(2);
				location <- any_location_in(free_space);
			}
			create filter_media {
				shape <- rectangle(30,20);
				location <- {myself.location.x, myself.location.y, myself.location.z-1.0};
			}
			create outlet{
				shape <- circle(1);
				location <- {myself.location.x + 13.0, myself.location.y, 0};
			}
			create gravel {
				shape <- rectangle(30,20);
				location <- {myself.location.x, myself.location.y, myself.location.z-2.0};
				
			}	
		}
		
		
		create lawn { // haut
		    shape <- rectangle(75, (75.0/2) - (first(NBSS).location.y - init_free_space.location.y) - 20.0/2) 
		             at_location {init_free_space.location.x, 
		                          first(NBSS).location.y + 20.0/2 + ((init_free_space.location.y + 75.0/2) - (first(NBSS).location.y + 20.0/2)) / 2};
		}
		
		create lawn { // bas
		    shape <- rectangle(75, (75.0/2) + (first(NBSS).location.y - init_free_space.location.y) - 20.0/2) 
		             at_location {init_free_space.location.x, 
		                          first(NBSS).location.y - 20.0/2 - ((first(NBSS).location.y - 20.0/2) - (init_free_space.location.y - 75.0/2)) / 2};
		}
		
		create lawn { // gauche
		    shape <- rectangle((75.0/2) + (first(NBSS).location.x - init_free_space.location.x) - 30.0/2, 20) 
		             at_location {first(NBSS).location.x - 30.0/2 - ((first(NBSS).location.x - 30.0/2) - (init_free_space.location.x - 75.0/2)) / 2,
		                          first(NBSS).location.y};
		}
		
		create lawn { // droite
		    shape <- rectangle((75.0/2) - (first(NBSS).location.x - init_free_space.location.x) - 30.0/2, 20) 
		             at_location {first(NBSS).location.x + 30.0/2 + ((init_free_space.location.x + 75.0/2) - (first(NBSS).location.x + 30.0/2)) / 2,
		                          first(NBSS).location.y};
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
		do add_background_geometries(grass,up_grass);
		do add_background_geometries(flower,up_flower);
		do add_background_geometries(filter_media, up_filter_media);
		do add_background_geometries(gravel, up_gravel);
		do add_background_geometries(swale, up_swale);
		
		do add_background_geometries(shrubs_plants, up_shrubs_plants);
		//do add_geometries_to_send(grass, up_grass);
		do add_background_geometries(trash, up_trash);
		do add_background_geometries(weeds, up_weeds);
		do add_background_geometries(vegetal_waste,up_vegetal_waste);
		do add_background_geometries(ponding_area,up_ponding_area);
		do add_background_geometries(lawn, up_lawn);
		do add_background_geometries(lawn_mower, up_lawn_mower);
	}
	
	
	//action that defines the different unity properties
	action define_properties {
		unity_aspect default_aspect <- geometry_aspect(1.0,#green,precision);
		up_default <- geometry_properties("default","",default_aspect,#no_interaction,false);
		unity_properties << up_default;

		/* Water flow */
		unity_aspect rain_aspect <- prefab_aspect("Prefabs/RainMaker/Prefab/RainPrefab",1.0,0.0,1.0,0.0,precision);
		up_rain <- geometry_properties("rain","rain",rain_aspect,#no_interaction,false);
		unity_properties << up_rain;
		
		
		/* NBSS components */
		unity_aspect filter_media_aspect <- geometry_aspect(1.0,#saddlebrown,precision);
		up_filter_media <- geometry_properties("filter_media","filter_media",filter_media_aspect,#ray_interactable,false);
		unity_properties << up_filter_media;
		
		unity_aspect gravel_aspect <- geometry_aspect(1.0, #slategrey, precision); //à voir si je définis la sous-couche en prefab ou non
		up_gravel <- geometry_properties("gravel","gravel",gravel_aspect,#no_interaction,false);
		unity_properties << up_gravel;
		
		unity_aspect ponding_area_aspect <- geometry_aspect(0.2, #blue, precision);
		up_ponding_area <- geometry_properties("ponding_area","ponding_area",ponding_area_aspect,#no_interaction,false);
		unity_properties << up_ponding_area;
		
		unity_aspect inlet_aspect <- geometry_aspect(0.75,#gray,precision);
		up_inlet <- geometry_properties("inlet","inlet",inlet_aspect,#ray_interactable,false);
		unity_properties << up_inlet;
		
		unity_aspect outlet_aspect <- geometry_aspect(0.75,#gray,precision);
		up_outlet <- geometry_properties("outlet","outlet",outlet_aspect,#ray_interactable,false);
		unity_properties << up_outlet;
		
		unity_aspect swale_aspect <- geometry_aspect(0.5,#green,precision);
		up_swale <- geometry_properties("swale","swale",swale_aspect,#ray_interactable,false);
		unity_properties << up_swale;
		

		/* Vegetation */
		unity_aspect trees_aspect <- prefab_aspect("Prefabs/Snowy_Low_Poly_Trees/Pine_NoSnow1",1.0,0.0,1.0,0.0,precision);
		up_trees <- geometry_properties("trees","trees",trees_aspect,#ray_interactable,false);
		unity_properties << up_trees;
		
		unity_aspect shrubs_aspect <- prefab_aspect("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Bush_1_1",1.0,0.0,1.0,0.0,precision);
		up_shrubs_plants <- geometry_properties("shrubs_plants","shrubs_plants",shrubs_aspect,#ray_interactable,false);
		unity_properties << up_shrubs_plants;
		
		unity_aspect grass_aspect <- prefab_aspect("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Grass_1_1",1.0,0.0,1.0,0.0,precision);
		up_grass <- geometry_properties("grass","grass",grass_aspect,#no_interaction,false);
		unity_properties << up_grass;
		
		unity_aspect lawn_aspect <- geometry_aspect(0.15,#green,precision);
		up_lawn <- geometry_properties("lawn","lawn",lawn_aspect,#no_interaction,false);
		unity_properties << up_lawn;
	
		unity_aspect flower_aspect <- prefab_aspect("Prefabs/DEMOLowPolyFlowers/Prefabs/SM_Hyacinth_PastellBlue_Big",1.0,0.0,1.0,0.0,precision);
		up_flower <- geometry_properties("flower","flower",flower_aspect,#no_interaction,false);
		unity_properties << up_flower;

	/* Trash/Invasive vegetation */
		unity_aspect vegetal_waste_aspect <- prefab_aspect("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Plant_1_1",1.0,0.0,1.0,0.0,precision);
		up_vegetal_waste <- geometry_properties("vegetal_waste","vegetal_waste",vegetal_waste_aspect,#ray_interactable,false);
		unity_properties << up_vegetal_waste;
		
		unity_aspect trash_aspect <- prefab_aspect("Prefabs/Mess Maker Free/Low Poly/Cans/Soda Can Green Crushed",1.0,0.0,1.0,0.0,precision);
		up_trash <- geometry_properties("trash","trash",trash_aspect,#ray_interactable,false);
		unity_properties << up_trash;
		
		unity_aspect weeds_aspect <- prefab_aspect("Prefabs/Parks And Nature Pack/Prefab/GrassE",1.0,0.0,1.0,0.0,precision);
		up_weeds <- geometry_properties("weeds","weeds",weeds_aspect,#ray_interactable,false);
		unity_properties << up_weeds;
		
		/* Interaction tools */
		unity_aspect lawn_mower_aspect <- prefab_aspect("Prefabs/Power Garden Tools/Prefabs/LawnMower",1.0,0.0,1.0,0.0,precision);
		up_lawn_mower <- geometry_properties("lawn_mower","lawn_mower",lawn_mower_aspect,#ray_interactable,false);
		unity_properties << up_lawn_mower;
		
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