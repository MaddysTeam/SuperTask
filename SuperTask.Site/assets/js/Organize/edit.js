$(function () {

	$('#jstreeModal')
		.on('click.jstree', function (e) {
			e.stopImmediatePropagation();
		})
		.on('changed.jstree', function (e, data) {
			$('#ParentName').empty(); // 清空所有元素
			$('#ParentName').append('<span class="tag label label-info">' + data.instance.get_node(data.selected).text + '<span data-role="remove"></span></span>');
			$('#ParentName span[data-role="remove"]').on('click', function () {
				$('#ParentName').empty();
				$('#ParentID').val('');
			}) //	删除
			$('#ParentID').val(data.instance.get_node(data.selected).id); // 获取上级部门id
			$('#dn-toggle').trigger('click'); // 点击之后隐藏
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
			'plugins': ['types'],
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
		}); //	上级部门


	//	自动补全

	var states = new Bloodhound({
		datumTokenizer: Bloodhound.tokenizers.obj.whitespace('RealName'),
		queryTokenizer: Bloodhound.tokenizers.whitespace,
		remote: {
			url: '/Organize/GetUser?query=%QUERY',
			wildcard: '%QUERY',
		}
	});

	states.initialize();

	//	部门负责人

	$('#chargeLeaderCombo .typeahead').typeahead({
		hint: true,
		highlight: true,
		minLength: 1
	}, {
		name: 'states',
		display: 'RealName',
		source: states.ttAdapter()
	}).bind('typeahead:select', function (e, item) {
		$('#chargeLeaderName .twitter-typeahead').siblings().remove();
		$('#chargeLeaderName .twitter-typeahead').hide();
		$('#chargeLeaderName').prepend('<span class="tag label label-info">' + item.RealName + '<span data-role="remove"></span></span>');
		$('#chargeLeaderName span[data-role="remove"]').on('click', function () {
			$('#chargeLeaderName .twitter-typeahead').show();
			$('#chargeLeaderName .twitter-typeahead .tt-input').val('');
			$('#chargeLeaderName .twitter-typeahead').siblings().remove();
			console.log($('#chargeLeaderName .typeahead'))
			$('#ChargeLeader').val('');
		})
		$('#ChargeLeader').val(item.Id);
	})

	//	部门领导

	$('#leaderCombo .typeahead').typeahead({
		hint: true,
		highlight: true,
		minLength: 1
	}, {
		name: 'states',
		display: 'RealName',
		source: states.ttAdapter()
	}).bind('typeahead:select', function (e, item) {
		$('#leaderName .twitter-typeahead').siblings().remove();
		$('#leaderName .twitter-typeahead').hide();
		$('#leaderName').prepend('<span class="tag label label-info">' + item.RealName + '<span data-role="remove"></span></span>');
		$('#leaderName span[data-role="remove"]').on('click', function () {
			$('#leaderName .twitter-typeahead').show();
			$('#leaderName .twitter-typeahead .tt-input').val('');
			$('#leaderName .twitter-typeahead').siblings().remove();
			console.log($('#leaderName .typeahead'))
			$('#Leader').val('');
		})
		$('#Leader').val(item.Id);
	})

	ajaxSubmitForm($('.modal-dialog form'));	//	提交表单
})


function afterDialogSuccess() {
	$('#firstModal').modal('hide');
}