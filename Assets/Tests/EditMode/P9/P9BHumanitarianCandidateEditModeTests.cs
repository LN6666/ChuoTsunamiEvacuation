using System.Linq;
using NUnit.Framework;

public class P9BHumanitarianCandidateEditModeTests
{
    [Test]
    public void P8EHandoffFilesAreAvailableForP9B()
    {
        P9BP8HandoffAvailability availability = P9BDataLoader.VerifyP8HandoffAvailability();

        Assert.IsTrue(availability.allExpectedFilesPresent, availability.summary);
        Assert.AreEqual(6, availability.presentFileCount);
        Assert.IsFalse(availability.affectsGameplaySuccessFailure);
    }

    [Test]
    public void HumanitarianCandidateHandoffLoadsAllOneHundredTenRecords()
    {
        P9BLoadResult<P9BHumanitarianCandidateMarkerCollection> result = P9BDataLoader.LoadP8HumanitarianCandidateMarkers();

        Assert.IsTrue(result.success, result.summary);
        Assert.AreEqual(110, result.data.totalCandidates);
        Assert.AreEqual(110, result.data.records.Length);
        Assert.AreEqual(28, result.data.namedMarkerCount);
        Assert.AreEqual(82, result.data.idOnlyMarkerCount);
        Assert.IsTrue(result.data.allCandidatesNonOfficial);
        Assert.IsTrue(result.data.allCandidatesRequireNonOfficialWarning);
        Assert.IsFalse(result.data.selectableGameplayEnabled);
        Assert.IsFalse(result.data.affectsGameplaySuccessFailure);
    }

    [Test]
    public void HumanitarianCandidateRecordsRemainNonOfficialAndWarningRequired()
    {
        P9BHumanitarianCandidateMarkerCollection collection = P9BDataLoader.LoadP8HumanitarianCandidateMarkers().data;

        Assert.IsTrue(collection.records.All(record => !record.isOfficialShelter));
        Assert.IsTrue(collection.records.All(record => record.nonOfficialWarningRequired));
        Assert.IsTrue(collection.records.All(record => !record.selectableGameplayEnabled));
        Assert.IsTrue(collection.records.All(record => !record.affectsGameplaySuccessFailure));
    }

    [Test]
    public void HumanitarianMarkerRuntimeConfigPreservesP9BBoundary()
    {
        P9BHumanitarianMarkerRuntimeConfig config = P9BDataLoader.LoadHumanitarianMarkerRuntimeConfig().data;

        Assert.AreEqual(110, config.expectedTotalCandidates);
        Assert.AreEqual(28, config.expectedNamedCandidateCount);
        Assert.AreEqual(82, config.expectedIdOnlyCandidateCount);
        Assert.IsFalse(config.isOfficialShelter);
        Assert.IsTrue(config.nonOfficialWarningRequired);
        Assert.IsFalse(config.selectableGameplayEnabled);
        Assert.IsFalse(config.affectsGameplaySuccessFailure);
    }
}
