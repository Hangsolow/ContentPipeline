using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using AutoFixture;

namespace ContentPipelineSourceGeneratorTests.Utils;

static internal class AutoMockFunc
{
    public static Func<IFixture> EnableAutoMock => () =>
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoNSubstituteCustomization());
        return fixture;
    };
}
public class AutoMockAttribute : AutoDataAttribute
{
    //enables NSubstitute mocks from AutoMockAttribute
    public AutoMockAttribute() : base(AutoMockFunc.EnableAutoMock) { }
}

public class InlineAutoMockAttribute : InlineAutoDataAttribute
{
    public InlineAutoMockAttribute(params object[] values) : base(new AutoMockAttribute(), values)
    {
    }
}
