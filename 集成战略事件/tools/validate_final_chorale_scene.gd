extends SceneTree


func _init() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() < 1 or args.size() > 2:
		push_error("Usage: godot --headless --path <tool_project> -s validate_final_chorale_scene.gd -- <pck> [scene_path]")
		quit(1)
		return

	var pck_path := args[0]
	var scene_path := "res://IntegratedStrategyEvents/scenes/creature_visuals/final_chorale.tscn"
	if args.size() == 2:
		scene_path = args[1]

	if not ProjectSettings.load_resource_pack(pck_path):
		push_error("Failed to load PCK: %s" % pck_path)
		quit(2)
		return

	var packed_scene = ResourceLoader.load(scene_path)
	if packed_scene == null:
		push_error("Failed to load scene: %s" % scene_path)
		quit(3)
		return

	var instance = packed_scene.instantiate()
	if instance == null:
		push_error("Failed to instantiate scene: %s" % scene_path)
		quit(4)
		return

	var visuals = instance.get_node_or_null("%Visuals")
	if visuals == null:
		push_error("Creature scene does not expose %Visuals: %s" % scene_path)
		quit(5)
		return

	print("Loaded creature scene: ", scene_path, " instance=", instance.name, " visuals=", visuals.get_class())
	instance.free()
	quit()
