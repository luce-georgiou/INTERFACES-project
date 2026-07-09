model NBSSAM_model_VR

import "NBSS_AM_v2.1_forVR.gaml"

species unity_linker parent: abstract_unity_linker {
	string player_species <- string(unity_player);
	int max_num_players  <- 1;
	int min_num_players  <- 1;

	unity_property up_filter_media;
	unity_property up_rain;
	unity_property up_trees;
	unity_property up_inlet;
	unity_property up_NBSS;
	unity_property up_default;
	unity_property up_outlet;
	unity_property up_ponding_area;
	
	//mes agents ajoutés
	unity_property up_shrubs_plants;
	unity_property up_grass;
	unity_property up_flower;
	unity_property up_vegetal_waste;
	unity_property up_trash;
	unity_property up_weeds;
	unity_property up_lawn_mower;
	unity_property up_building;
	unity_property up_road;
	unity_property up_park;
	unity_property up_nbss_area;
	unity_property up_local_flora;

	bool do_send_world <- true;
	list<point> init_locations <- [{100.0, 0.0}]; //[any_location_in(init_free_space)];

	init {
		do define_properties;
		player_unity_properties <- [up_default];
		
		do add_background_geometries(building, up_building);
		do add_background_geometries(road, up_road);
		do add_background_geometries(park, up_park);
	}
	
	action define_properties {
		unity_aspect default_aspect <- geometry_aspect(1.0,#green,precision);
		up_default <- geometry_properties("default","",default_aspect,#no_interaction,false);
		unity_properties << up_default;
		
//		unity_aspect failure_event_aspect <- geometry_aspect(1.0,#green,precision);
//		up_failure_event <- geometry_properties("failure_event","failure_event",failure_event_aspect,#no_interaction,false);
//		unity_properties << up_failure_event;

		/* Water flow */
		unity_aspect rain_aspect <- prefab_aspect("Prefabs/RainMaker/Prefab/RainPrefab",1.0,0.0,1.0,0.0,precision);
		up_rain <- geometry_properties("rain","rain",rain_aspect,#no_interaction,false);
		unity_properties << up_rain;
		
		
		/* NBSS components */
//		unity_aspect NBSS_aspect <- geometry_aspect(0.15,#green,precision);
//		up_NBSS <- geometry_properties("NBSS","NBSS",NBSS_aspect,#no_interaction,false);
//		unity_properties << up_NBSS;

		unity_aspect nbss_area_aspect <- prefab_aspect("Prefabs/NBSSAreaPrefab",1.0,0.0,1.0,0.0,precision);
		up_nbss_area <- geometry_properties("nbss_area","nbss_area",nbss_area_aspect,#ray_interactable,false);
		unity_properties << up_nbss_area;
		
		unity_aspect filter_media_aspect <- geometry_aspect(1.5,#saddlebrown,precision);
		up_filter_media <- geometry_properties("filter_media","filter_media",filter_media_aspect,#ray_interactable,false);
		unity_properties << up_filter_media;
		
		unity_aspect ponding_area_aspect <- geometry_aspect(0.1, #blue, precision);
		up_ponding_area <- geometry_properties("ponding_area","ponding_area",ponding_area_aspect,#no_interaction,false);
		unity_properties << up_ponding_area;
		
//		unity_aspect inlet_aspect <- geometry_aspect(0.75,#gray,precision);
//		up_inlet <- geometry_properties("inlet","inlet",inlet_aspect,#ray_interactable,false);
//		unity_properties << up_inlet;
//		
//		unity_aspect outlet_aspect <- geometry_aspect(0.75,#gray,precision);
//		up_outlet <- geometry_properties("outlet","outlet",outlet_aspect,#ray_interactable,false);
//		unity_properties << up_outlet;
		

		/* Vegetation */
		unity_aspect trees_aspect <- prefab_aspect("Prefabs/Snowy_Low_Poly_Trees/Pine_NoSnow1",1.0,0.0,1.0,0.0,precision);
		up_trees <- geometry_properties("trees","trees",trees_aspect,#ray_interactable,false);
		unity_properties << up_trees;
		
		unity_aspect shrubs_aspect <- prefab_aspect("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Bush_1_1",1.0,0.0,1.0,0.0,precision);
		up_shrubs_plants <- geometry_properties("shrubs_plants","shrubs_plants",shrubs_aspect,#ray_interactable,false);
		unity_properties << up_shrubs_plants;
		
//		unity_aspect lawn_aspect <- geometry_aspect(0.1,#green,precision);
//		up_lawn <- geometry_properties("lawn","lawn",lawn_aspect,#no_interaction,false);
//		unity_properties << up_lawn;
	
		unity_aspect flower_aspect <- prefab_aspect("Prefabs/DEMOLowPolyFlowers/Prefabs/SM_Dandelion_Small",1.0,0.0,1.0,0.0,precision);
		up_flower <- geometry_properties("flower","flower",flower_aspect,#no_interaction,false);
		unity_properties << up_flower;
		
		unity_aspect local_flora_aspect <- prefab_aspect("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Grass_1_2",1.0,0.0,1.0,0.0,precision);
		up_local_flora <- geometry_properties("local_flora","local_flora",local_flora_aspect,#no_interaction,false);
		unity_properties << up_local_flora;
		
		
		/* Trash/Invasive vegetation */
		unity_aspect vegetal_waste_aspect <- prefab_aspect("Prefabs/FreeVegetation-LowPolyNature/FreeVegetation/Prefabs/Plant_1_1",1.0,0.0,1.0,0.0,precision);
		up_vegetal_waste <- geometry_properties("vegetal_waste","vegetal_waste",vegetal_waste_aspect,#ray_interactable,false);
		unity_properties << up_vegetal_waste;
		
		unity_aspect trash_aspect <- geometry_aspect(0.5,#red,precision);
		//unity_aspect trash_aspect <- prefab_aspect("Prefabs/Mess Maker Free/Low Poly/Cans/Soda Can Green Crushed",1.0,0.0,1.0,0.0,precision); //pb avec ce prefab
		up_trash <- geometry_properties("trash","trash",trash_aspect,#ray_interactable,false);
		unity_properties << up_trash;
		
		unity_aspect weeds_aspect <- prefab_aspect("Prefabs/Parks And Nature Pack/Prefab/GrassE",1.0,0.0,1.0,0.0,precision);
		up_weeds <- geometry_properties("weeds","weeds",weeds_aspect,#ray_interactable,false);
		unity_properties << up_weeds;
		
		/* Interaction tools */
		unity_aspect lawn_mower_aspect <- prefab_aspect("Prefabs/Power Garden Tools/Prefabs/LawnMower",1.0,0.0,1.0,0.0,precision);
		up_lawn_mower <- geometry_properties("lawn_mower","lawn_mower",lawn_mower_aspect,#ray_interactable,false);
		unity_properties << up_lawn_mower;
		
		/* Urban environment */
		unity_aspect road_aspect <- geometry_aspect(0.2,#gray,precision);
		up_road <- geometry_properties("road","",road_aspect,#no_interaction,false);
		unity_properties << up_road;
		
		unity_aspect building_aspect <- geometry_aspect(10,#gray,precision);
		up_building <- geometry_properties("building","",building_aspect,#no_interaction,false);
		unity_properties << up_building;
		
		unity_aspect park_aspect <- geometry_aspect(0.2,#darkgreen,precision);
		up_park <- geometry_properties("park","park",park_aspect,#no_interaction,false);
		unity_properties << up_park;
//		
	}
	
	reflex send_geometries {

		do add_geometries_to_send(shrubs_plants, up_shrubs_plants);
		do add_geometries_to_send(trash, up_trash);
		do add_geometries_to_send(weeds, up_weeds);
		do add_geometries_to_send(flower, up_flower);
		//do add_geometries_to_send(lawn, up_lawn);
		do add_geometries_to_send(lawn_mower, up_lawn_mower);
		do add_geometries_to_send(nbss_area, up_nbss_area);
		do add_geometries_to_send(local_flora, up_local_flora);
	}
	
	// sending messages
//	reflex send_messages_to_Unity when: (one_of(failure_event).last_failure = current_date) and (one_of(failure_event).last_failure > starting_date) and (one_of(failure_event).failure_happened = false) {
//		write "sending message";
//		//do send_message players: unity_player as list mes: ["name_failure_event":: last(failure_event).my_name,"component":: last(failure_event).impacted_agent ];
//	}
	
	// modify state of species according to health/biodiv
	reflex send_agents when: not empty(unity_player) {
		//list<int> fqt_inlet <- inlet collect (each.function_attributes["my_fqt"]);
		//list<int> biodiv_inlet <- inlet collect (each.function_attributes["my_biodiv"]);
		//list<int> fqt_outlet <- outlet collect (each.function_attributes["my_fqt"]);
		list<float> rain_intensity <- rain collect float(each.runoff.my_flow);
		list<string> tree_seasons <- trees collect current_season; 
		list<string> rain_seasons <- rain collect current_season;
		list<int> fqt_fm <- filter_media collect (each.function_attributes["my_fqt"]);
		list<float> fm_sediments <- filter_media collect (each.partpoll_acc);
		//list<float> lawn_height <- lawn collect each.height;
		//list<string> lawn_seasons <- lawn collect current_season;
//		list<string> failures_name <- failure_event collect each.my_name;
//		list<string> failures_aff_component <- failure_event collect each.impacted_agent;
//		write failures_name;
//		write failures_aff_component;

		list<float> water_level_pond <- ponding_area collect (each.water_level);
		//write water_level_pond;
		
//		map<string,list<unknown>> atts_inlet <-  [
//			"fqt_inlet":: fqt_inlet
//		]; 
//		map<string,list<int>> atts_outlet <- ["fqt_outlet":: fqt_outlet];
		map<string,list<unknown>> atts_fm <-  [
			"fqt_fm":: fqt_fm,
			"sediments_fm":: fm_sediments
		]; 
		map<string,list<unknown>> atts_rain <- [
			"rain_intensity":: rain_intensity,
			"rain_seasons":: rain_seasons
		];
//		map<string,list<unknown>> atts_lawn <- [
//			"lawn_height":: lawn_height,
//			"lawn_seasons":: lawn_seasons
//		];
		//map<string, list<string>> atts_rain_seasons <- ["rain_seasons"::rain_seasons];
		map<string, list<string>> atts_trees <- ["tree_seasons"::tree_seasons];
//		map<string,list<string>> atts_failures <- [
//			"failure_name":: failures_name,
//			"impacted_component":: failures_aff_component
//		];
//		if one_of(inlet).my_failures != [] {
//			list<string> failures_inlet <- inlet collect last(each.my_failures).my_name;
//			atts_inlet <- atts_inlet + ("failures_inlet":: failures_inlet);
//			
//		}
//		if one_of(outlet).my_failures != [] {
//			list<string> failures_outlet <- outlet collect last(each.my_failures).my_name;
//			atts_outlet <- atts_outlet + ("failures_outlet":: failures_outlet);
//			
//		}
		
		map<string,list<unknown>> atts_ponding_area <- ["water_level":: water_level_pond];
		//at every step, we send the dynamic_punctual_agent agents with the up_car properties and the attributes "atts" 
//		do add_geometries_to_send(inlet,up_inlet,atts_inlet);
//		do add_geometries_to_send(outlet,up_outlet,atts_outlet);	
		do add_geometries_to_send(rain,up_rain,atts_rain);
		//do add_geometries_to_send(rain,up_rain,atts_rain_seasons);
		do add_geometries_to_send(trees,up_trees,atts_trees);
		//do add_geometries_to_send(lawn, up_lawn, atts_lawn);
		do add_geometries_to_send(ponding_area, up_ponding_area, atts_ponding_area);
		do add_geometries_to_send(filter_media, up_filter_media, atts_fm);
		//do add_geometries_to_send(failure_event, up_failure_event, atts_failures);
		
	}
	
	// Maintenance practices and their impact on biodiv/costs/vegetation health
	action maintenance_remove(string id) {
		agent ag <- (trash + weeds + vegetal_waste) first_with (each.name = id) ;
		if (ag != nil) {
			ask ag {
				remove key: self from: myself.geometries_to_send;
				do die;
			}
		}
	}
//	action maintenance_repair(string id) {
//		component ag <- (engineered_component + vegetal_component) first_with (each.name = id) ;
//		if (ag != nil) {
//			ask ag {
//				if function_attributes["my_health"] < 3 {
//					function_attributes["my_health"] <- function_attributes["my_health"] + 1;
//				}
//				else {
//					function_attributes["my_health"] <- function_attributes["my_health"] - 1;
//				}
//				price <- price + 1;
//			}
//		}
//	}
	action add_veg(string id) {
		agent ag <- (shrubs_plants + grass + trees) first_with (each.name = id) ;
		if (ag != nil) {
			ask ag {
				create ag;
				//add ag to: myself.geometries_to_send; //pas sûre
			}
		}
	}
	action water_plants(string id) {
		//if (current_season = "summer" and time_since_last_water >= 2) {
			// si trop arrosée, perds de la santé, sinon en gagne (et affecte apparence plantes) aussi dépend des saisons
		//}
	}
	action mow_grass_trees(string id) {
		agent ag <- (trees + grass) first_with(each.name = id);
		if ag != nil {
			create vegetal_waste {
				location <- rnd({1.0, 0.0, 0.0}); //à déterminer mais dans ponding area
			}
			// supprimer une partie des feuilles/de l'herbe au niveau du sol/des arbres
		}
	}
	
	action vegetal_waste_spawner(int veg_waste_amnt) {
		create weeds number: veg_waste_amnt {location <- any_location_in(one_of(ponding_area).shape);}
	}
	
	action mow_lawn(string id) {
		lawn_mower tool <- lawn_mower first_with(each.name = id); 
		if tool != nil {
			ask lawn {
				height <- height - 0.5;
			}
			do vegetal_waste_spawner(10);
		}
	}
	
//	action change_color
//	//impact décisions sur envir/pluie (si failure_event -> impact aussi)
//	reflex dying_component when: is_failure_event { //lien failure event
//		//selon état (3, 2, 1, 0), composant devient rouge, jaune, orange -> changer aspect des species
//		is_failure_event <- false;
//	}

	// faire pousser des mauvaises herbes quand la végétation n'est pas saine
	reflex invasive_weeds {
		ask vegetation_cover {
			if invasive = true {
				create weeds {location <- any_location_in(one_of(NBSS).shape) every(100 #cycle);}
			}
		}
	}
	// accumulation de déchets
	reflex trash_acc {
		ask ext_time_failure {
			if (my_name = "trash_acc" and (cycle mod (my_frequency * 7)) = 0) { // toutes les 12 semaines on ajoute un déchet
				create trash {
					shape <- circle(0.1);
					location <- any_location_in(one_of(NBSS).shape);
				}
			}
		}
	}
	// lawn growing
//	reflex lawn_growth {
//		if (current_season = "spring") { // herbe pousse plus vite au printemps = plus de tonte nécessaire
//			if (cycle mod (1*7) = 0) {
//				ask lawn {
//					height <- height + 0.01;
//				}
//			}
//		}
//		else {
//			if (cycle mod (2*7) = 0) {
//				ask lawn {
//					height <- height + 0.001;
//				}
//			}
//		}
//	}
	
	// gestion des saisons
	
	// reflex biodiv -> selon état, fleurs apparaissent ou non
	reflex dying_biodiv {
		//selon état, réduire le nombre d'espèce dans l'environnement (et réduire fonctionnalité soil par ex) -> enlever agents des listes (uorg par ex)
	}
	reflex budget {
		//coût de maintenance, je sais pas trop pour l'instant
	}
	reflex sediment_acc {
		//quand valeur dans tableau potpall_acc augmente, ajouter une couche de sédiments (et transformer vegetal waste en sediments)
	}
	reflex send_message when: send_message {
		//write "Send message: ";
		//do send_message players: unity_player as list mes: ["message_init"::"Mmmh certaines noues semblent ne pas fonctionner correctement..."];
		do send_message players: unity_player as list mes: messages;
		send_message <- false;
		messages <- [];
	}
	action receive_message(string id, string mes) {
		write "Player " + id + " send the message: " + mes;
		if (mes = "skip") {
			do_skip <- true; 
		}
		else if (mes = "scenario1") {
			launch_sc1 <- true;
		}
		else if (mes = "scenario2") {
			launch_sc2 <- true;
		}
		else { //On traite le score en fin de phase
			score <- float(mes);
			score <- float(mes replace (",", "."));
			write "score : " + score;
		}
	}
	
	reflex is_flooding when: ponding_area one_matches (each.is_obstructed) {
		ask ponding_area where (each.is_obstructed) {
			//write my_name;
			if ( water_level < 0.5) { //cycle mod (1*4) = 0 and
				water_level <- water_level + 0.05;
			}
//			else {
//				water_level <- 0.0;
//			}
		}
	}
	
	// scénario 1 actions
	action curage(string id) {
		filter_media fm <- (filter_media first_with (each.name = id));
		if (fm != nil) {
			ask fm {
				if (function_attributes["my_fqt"] <= 1) {
					function_attributes["my_fqt"] <- 2;
					partpoll_acc <- 0.0;
					ask ponding_area where (each.is_obstructed) { // condition à modif selon si transit ou non, et chemin de l'eau
						is_obstructed <- false;
						water_level <- 0.0;
					}
					//score <- score + 30.0;
					//weight_score <- 30;
					messages <- messages + ["init_":: "regular_state"];
					messages <- messages + ["add_to_score":: "45.0"];
					send_message <- true;	
				}
			}
		}
	}
	
	// scénario 2 actions
	action arroser(string id) {
		filter_media fm <- (filter_media first_with (each.name = id));
		if (fm != nil) {
			ask ponding_area where (each.my_NBSS = fm.my_NBSS) {
				water_level <- water_level + 0.2;
			}
			//weight_score <- 15;
			messages <- messages + ["add_to_score":: string(15)];
			send_message <- true;
		}
	}
	action planter_flore_locale(string id) {
		filter_media ag_fm <- filter_media first_with (each.name = id);
	    
	    NBSS target_nbss <- nil;
	    if (ag_fm != nil) {
	        target_nbss <- ag_fm.my_NBSS;
	    }
	    
	    if (target_nbss != "") {
	        list<filter_media> fm_valides <- filter_media where (each.my_NBSS = target_nbss);
	        
	        agent zone_cible <- one_of(fm_valides);
	        
	        if (zone_cible != nil) {
	            create local_flora number: 25 {
	                location <- any_location_in(zone_cible.shape);
	            }
	            write "local_flora created";
	        }
	        //weight_score <- 25;
			messages <- messages + ["add_to_score":: string(25)];
			send_message <- true;
	    }	
	}
//	action planter_barriere_veg(string id) {
//		nbss_area ag <- nbss_area first_with (each.name = id);
//		if (ag != nil) {
//			create shrubs_plants;
//		}
//	}

//	action point_gain(string id) {
//		agent ag <- (agent first_with (each.name = id));
//		if (ag != nil) {
//			weight_score <- 20.0;
//			messages <- messages + ["add_to_score":: string(weight_score)];
//			send_message <- true;
//		}
//	}
}



species unity_player parent: abstract_unity_player{
	float player_size <- 1.0;
	rgb color <- #red;
	float cone_distance <- 10.0 * player_size;
	float cone_amplitude <- 90.0;
	float player_rotation <- 90.0;
	bool to_display <- true;
	float z_offset <- 2.0;
	aspect default {
		if to_display {
			if selected {
				 draw circle(player_size) at: location + {0, 0, z_offset} color: rgb(#blue, 0.5);
			}
			draw circle(player_size/2.0) at: location + {0, 0, z_offset} color: color ;
			draw player_perception_cone() color: rgb(color, 0.5);
		}
	}
	
//	reflex stay_in_bounds { // marche pas
//        if !(init_free_space overlaps self) {
//            write "out of bounds";
//            location <- closest_points_with(init_free_space.location, self)[0]; // ou 1
//        }
//    }
}

experiment vr_xp parent:"Interface (EN)" autorun: true type: unity {
	float minimum_cycle_duration <- 0.1;
	string unity_linker_species <- string(unity_linker);
	list<string> displays_to_hide <- ["Rain", "Inlet", "Ponding area", "Vegetation cover", "Filter media", "Performance", map];
	float t_ref;

	action create_player(string id) {
		ask unity_linker {
			do create_player(id);
			
			do build_invisible_walls(player: last(unity_player), //player to send the information to
			id: "wall_for_free_area", //id of the walls
			height: 40.0, //height of the walls
			wall_width: 1.0, //width of the walls
			geoms: [init_free_space]);
		}
	}

	action remove_player(string id_input) {
		if (not empty(unity_player)) {
			ask first(unity_player where (each.name = id_input)) {
				do die;
			}
		}
	}

	output {
		 
		 display map_VR parent: map {

			 species unity_player;
			 event #mouse_down{
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
