using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bricksandmortarstudio.checkinextensions
{
    //TODO FINISH
    [CategoryField("Headcount Category", "The category to store your metrics in", false, "Rock.Model.MetricCategory", required:true, key:"headcountcategory")]
    [DisallowConcurrentExecution]
    public class SetupHeadcountMetric : IJob
    {
        private MetricService _metricService;
        private MetricCategoryService _metricCategoryService;
        private int? _categoryEntityTypeId;
        private RockContext _rockContext;
        private CategoryService _categoryService;
        private int _checkInTemplateId;
        private List<int> _seenGroupTypeIds = new List<int>(); 

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SetupHeadcountMetric()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            _rockContext = new RockContext();
            var groupTypeService = new GroupTypeService(_rockContext);
            _metricService = new MetricService(_rockContext);
            _metricCategoryService = new MetricCategoryService(_rockContext);
            _categoryService = new CategoryService(_rockContext);
            _categoryEntityTypeId = EntityTypeCache.GetId(typeof (MetricCategory));
            var rootcategoryCategoryGuid = dataMap.GetString("headcountcategory").AsGuid();
            var rootCategory = new CategoryService(_rockContext).Get(rootcategoryCategoryGuid);
            if (rootCategory == null)
            {
                throw new Exception("No root category found to store the headcount metrics in.");
            }
            _checkInTemplateId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE).Id;
            var startingGroupTypes = groupTypeService.Queryable().Where(g => g.GroupTypePurposeValueId == _checkInTemplateId).ToList();
            if (!startingGroupTypes.Any())
            {
                return;
            }

            foreach (var groupType in startingGroupTypes)
            {
                _seenGroupTypeIds.Add(groupType.Id);
                var descendentGroupTypes = groupType.ChildGroupTypes.Where(cgt => !_seenGroupTypeIds.Contains(cgt.Id));
                if (descendentGroupTypes != null)
                {
                    ProcessDescendents(descendentGroupTypes.ToList(), rootCategory);
                }
            }

            _rockContext.SaveChanges();
        }

        private void ProcessDescendents(IList<GroupType> descendentGroupTypes, Category rootCategory)
        {
            foreach (var groupType in descendentGroupTypes)
            {
                _seenGroupTypeIds.Add(groupType.Id);
                if (!groupType.Groups.Any() && !groupType.ChildGroupTypes.Any(g => g.Groups.Any()))
                {
                    continue;
                }
                {
                    var category = _categoryService.Queryable().FirstOrDefault(c => c.ForeignGuid == groupType.Guid) ??
                                   MakeGroupTypeCategory(groupType, rootCategory);
                    var groups = groupType.Groups.ToList();
                    foreach (var group in groups)
                    {
                        if (
                            !_metricService.Queryable()
                                .Any(
                                    m =>
                                        m.MetricCategories.Any(c => c.CategoryId == category.Id) &&
                                        m.ForeignGuid != null && m.ForeignGuid == group.Guid))
                        {
                            var metric = MakeGroupMetric(group);
                            _metricService.Add(metric);
                            var metricCategory = new MetricCategory
                            {
                                CategoryId = category.Id,
                                Metric = metric
                            };
                            _metricCategoryService.Add(metricCategory);
                        }
                    }
                    _rockContext.SaveChanges();

                    if (groupType.ChildGroupTypes != null)
                    {
                        ProcessDescendents(
                            groupType.ChildGroupTypes.Where(
                                g => g.GroupTypePurposeValueId == _checkInTemplateId && !_seenGroupTypeIds.Contains(g.Id))
                                .ToList(), category);
                    }
                }
            }
        }

        private static Metric MakeGroupMetric(Group group)
        {
            var metric = new Metric
            {
                Title = @group.Name,
                YAxisLabel = "Headcount",
                Description = String.Format( "Headcount for {0}", @group.Name ),
                ForeignGuid = group.Guid,
                SourceValueTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id
            };
            return metric;
        }

        private Category MakeGroupTypeCategory(GroupType groupType, Category parentCategory, int order = 0)
        {
            var category = new Category
            {
                Name = groupType.Name,
                EntityTypeId = _categoryEntityTypeId.Value,
                ParentCategoryId = parentCategory.Id,
                Description = "Generated by the Setup Headcount Metric Job",
                Order = order,
                IsSystem = false,
                ForeignGuid = groupType.ForeignGuid
            };
            var categoryService = new CategoryService(_rockContext);
            categoryService.Add(category);
            _rockContext.SaveChanges();
            return category;
        }
    }
}