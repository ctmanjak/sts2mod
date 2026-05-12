extends SceneTree


func _init() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() != 1 and args.size() != 2:
		push_error("Usage: godot --headless --path <project> -s <script> -- [resource_pack] <resource_path>")
		quit(1)
		return

	var resource_path := String(args[0])
	if args.size() == 2:
		var pack_path := String(args[0])
		if not ProjectSettings.load_resource_pack(pack_path):
			push_error("Failed to load resource pack: %s" % pack_path)
			quit(1)
			return
		resource_path = String(args[1])

	var resource := ResourceLoader.load(resource_path)
	if resource == null:
		push_error("Failed to load resource: %s" % resource_path)
		quit(1)
		return

	print("Loaded ", resource_path, " as ", resource.get_class())
	if resource is Texture2D:
		print("Texture size: ", resource.get_width(), "x", resource.get_height())

	quit()
