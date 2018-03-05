$(function () {

	$('#jstreeModal')
	.on('click.jstree', function (e) {
		e.stopImmediatePropagation();
	})
	.on('changed.jstree', function (e, data) {
		
	})
	.jstree({
		'core': {
			'animation': 0,
			'multiple': false,
			'check_callback': true,
			'themes': {},
			'data': {
				'url': function (node) {
					return '/Organize/GetTreeList'; //	获取部门链接
				},
				'data': function (node) {
					return { 'id': node.id };
				}
			}
		},
		'contextmenu': {
			items: function (node) {
				var tmp = $.jstree.defaults.contextmenu.items();
				tmp.rename.label = '重命名';
				tmp.edit.label = '编辑'
				tmp.remove.label = '删除';

				tmp.remove.action = function (data) {
					var inst = $.jstree.reference(data.reference), obj = inst.get_node(data.reference);

					$.post('/ajax/voucher/department/delete', { 'department_id': obj.id, "_xsrf": util.getCookie("_xsrf") })
						 .done(function (d) {
						 	d = $.parseJSON(d);
						 	if (d.code == 0) {
						 		if (inst.is_selected(obj)) {
						 			inst.delete_node(inst.get_selected());
						 		} else {
						 			inst.delete_node(obj);
						 		}
						 		inst.refresh();
						 		util.msgShow("success", d.msg);
						 	} else {
						 		util.msgShow("error", d.msg);
						 		// inst.refresh();
						 	}
						 })
				};

				tmp.create.action = function (data) {
					console.log("create action")
					var inst = $.jstree.reference(data.reference), obj = inst.get_node(data.reference);
					inst.create_node(obj, {}, "last", function (new_node) {
						setTimeout(function () {
							inst.edit(new_node, "新部门", function (data) {

								$.post('/ajax/voucher/department/add', { 'parent_id': obj.id, 'name': data.text, "_xsrf": util.getCookie("_xsrf") })
									 .done(function (d) {
									 	d = $.parseJSON(d);
									 	if (d.code == 0) {
									 		inst.set_id(data, "" + d.data);
									 		treeDataCollection.add(_.extend(node, { id: d.data }));
									 		self.parentView.departmentMappingView.refreshView();
									 		util.msgShow("success", d.msg);
									 		inst.refresh();

									 	} else {
									 		util.msgShow("error", d.msg);
									 		inst.refresh();
									 	}
									 })
									 .fail(function () {
									 	inst.refresh();
									 });
							});
						}, 0);
					});
				};

				tmp.rename.action = function (data) {
					var inst = $.jstree.reference(data.reference), obj = inst.get_node(data.reference);
					inst.edit(obj, obj.text, function (data) {
						var departmentId = obj.id;
						if (obj.text != obj.old) {
							$.post('/ajax/voucher/department/update', { 'parent_id': obj.parent, department_id: obj.id, 'name': data.text, "_xsrf": util.getCookie("_xsrf") })
								 .done(function (d) {
								 	d = $.parseJSON(d);
								 	if (d.code == 0) {
								 		inst.set_id(data, departmentId);
								 		self.parentView.departmentMappingView.refreshView();
								 		util.msgShow("success", d.msg);
								 	} else {
								 		util.msgShow("error", d.msg);
								 	}
								 })
						}
					});
				};

				return {
					create: tmp.create,
					rename: tmp.rename,
					remove: tmp.remove
				};
			}
		},
		'plugins': ['contextmenu', 'types'],
		'types': {
			'root': {
				'icon': 'fa fa-desktop'
			},
			'default': {
				'icon': 'fa fa-university'
			},
			'database': {
				'icon': 'fa fa-database yellow-font'
			},
			'table': {
				'icon': 'fa fa-table green-font'
			},
			'view': {
				'icon': 'fa fa-search text-primary'
			},
			'procedure': {
				'icon': 'fa fa-play-circle green-font'
			},
			'key': {
				'icon': 'fa fa-key text-primary'
			},
			'folder': {
				'icon': 'fa fa-folder yellow-font'
			},
			'content': {
				'icon': 'fa fa-pencil-square-o text-primary'
			},
			'active': {
				'icon': 'fa fa-table green-font'
			},
		}
	});

})