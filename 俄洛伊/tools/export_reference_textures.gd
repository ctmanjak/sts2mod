extends SceneTree


func _init() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() < 2:
		push_error("Usage: godot --headless -s export_reference_textures.gd -- <base_pck> <output_dir>")
		quit(1)
		return

	var base_pck := args[0]
	var output_dir := args[1]
	DirAccess.make_dir_recursive_absolute(output_dir)

	if not ProjectSettings.load_resource_pack(base_pck, true):
		push_error("Failed to load resource pack: %s" % base_pck)
		quit(1)
		return

	var exports := {
		"ironclad_restsite_atlas.png": "res://animations/rest_site/ironclad/restsite_ironclad.png",
		"silent_restsite_atlas.png": "res://animations/rest_site/silent/restsite_silent.png",
		"defect_restsite_atlas.png": "res://animations/rest_site/defect/restsite_defect.png",
		"necrobinder_restsite_atlas.png": "res://animations/rest_site/necrobinder/restsite_necrobinder.png",
		"osty_restsite_atlas.png": "res://animations/rest_site/necrobinder/restsite_osty.png",
		"regent_restsite_atlas.png": "res://animations/rest_site/regent/restsite_regent.png",
	}

	var failed := 0
	for file_name in exports:
		var resource_path: String = exports[file_name]
		var texture := ResourceLoader.load(resource_path) as Texture2D
		if texture == null:
			push_error("Failed to load texture: %s" % resource_path)
			failed += 1
			continue

		var image := texture.get_image()
		if image == null:
			push_error("Texture has no image: %s" % resource_path)
			failed += 1
			continue

		var err := image.save_png(output_dir.path_join(file_name))
		if err != OK:
			push_error("Failed to save %s: %s" % [file_name, err])
			failed += 1
		else:
			print("Exported ", output_dir.path_join(file_name))

	quit(1 if failed > 0 else 0)
