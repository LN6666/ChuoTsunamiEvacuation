using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P5DRealQualifiedGameplayDataTests
{
    [Test]
    public void SourceModeDefaultRemainsTestAndRealQualifiedIsRecognized()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig defaultConfig =
            ShelterSourceConfigLoader.CreateDefaultConfig();

        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, defaultConfig.sourceMode);

        ShelterSourceConfigLoader.ShelterSourceConfig realQualifiedConfig =
            ShelterSourceConfigLoader.CreateDefaultConfig();
        realQualifiedConfig.sourceMode = ShelterSourceConfigLoader.RealQualifiedSourceMode;
        realQualifiedConfig.Sanitize();

        Assert.AreEqual(ShelterSourceConfigLoader.RealQualifiedSourceMode, realQualifiedConfig.sourceMode);
    }

    [Test]
    public void RealQualifiedLoaderReadsAssetsDataStaticJsonOnly()
    {
        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult result =
            RealQualifiedShelterDataLoader.LoadFromAssetsData();

        Assert.IsTrue(result.success);
        Assert.IsTrue(result.usesAssetsDataOnly);
        Assert.AreEqual("p5_b5_real_chuo_integrated_route_qualification", result.datasetId);
        Assert.GreaterOrEqual(result.recordCount, 31);
        Assert.AreEqual(27, result.selectableCount);
        Assert.That(result.osmAttribution, Does.Contain("OpenStreetMap"));
        Assert.That(result.osmAttribution, Does.Contain("Open Database License"));

        foreach (string sourcePath in result.sourcePaths)
        {
            Assert.That(Normalize(sourcePath), Does.Contain("/Assets/Data/"));
        }
    }

    [Test]
    public void RealQualifiedSourceModeLoadsSelectableQualifiedRecords()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();
        config.sourceMode = ShelterSourceConfigLoader.RealQualifiedSourceMode;

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(result.success);
        Assert.IsTrue(result.IsRealQualified);
        Assert.AreEqual(ShelterSourceConfigLoader.RealQualifiedSourceMode, result.sourceMode);
        Assert.AreEqual(0, result.testShelters.Length);
        Assert.AreEqual(0, result.realShelters.Length);
        Assert.GreaterOrEqual(result.realQualifiedShelters.Length, 31);
        Assert.AreEqual(27, result.realQualifiedLoadResult.selectableCount);
    }

    [Test]
    public void RealQualifiedSourceModeSmokeKeepsOnlyOfficialConfirmedRecordsPlayable()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();
        config.sourceMode = ShelterSourceConfigLoader.RealQualifiedSourceMode;

        ShelterDataSourceResolver.ShelterDataSourceResult result =
            ShelterDataSourceResolver.LoadFromConfig(config);

        Assert.IsTrue(result.success);
        Assert.IsTrue(result.IsRealQualified);
        Assert.AreEqual(ShelterSourceConfigLoader.RealQualifiedSourceMode, result.sourceMode);

        int selectableCount = 0;
        int debugOnlyCount = 0;
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord firstSelectable = null;

        foreach (RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record in result.realQualifiedShelters)
        {
            Assert.NotNull(record);

            bool selectableStatus =
                RealQualifiedShelterDataLoader.IsDefaultSelectableQualificationStatus(record.qualificationStatus);

            if (record.isSelectable)
            {
                selectableCount++;
                if (firstSelectable == null)
                {
                    firstSelectable = record;
                }
                Assert.IsTrue(selectableStatus, record.qualificationStatus);
                Assert.IsTrue(record.hasSafeMapping, record.gameplayShelterId);
                Assert.IsFalse(string.IsNullOrWhiteSpace(record.gameplayShelterId));
                Assert.IsFalse(string.IsNullOrWhiteSpace(record.plateauBuildingId));
                Assert.AreEqual(P5CStaticDataLoader.EstimatedPrototypeRouteLabel, record.routeDisclaimer);
                Assert.That(record.attribution, Does.Contain("OpenStreetMap"));
                Assert.That(record.attribution, Does.Contain("Open Database License"));
            }
            else
            {
                debugOnlyCount++;
                Assert.IsFalse(record.isSelectable);
                Assert.IsFalse(string.IsNullOrWhiteSpace(record.nonSelectableReason));
                if (!selectableStatus)
                {
                    Assert.That(record.nonSelectableReason, Does.Contain("debug-only"));
                }
            }
        }

        Assert.AreEqual(result.realQualifiedLoadResult.selectableCount, selectableCount);
        Assert.Greater(selectableCount, 0);
        Assert.Greater(debugOnlyCount, 0);

        ShelterDataLoader.ShelterData[] gameplayShelters =
            ShelterGameplayDataMapper.MapRealQualifiedSheltersToGameplayData(result.realQualifiedShelters);
        Assert.AreEqual(selectableCount, gameplayShelters.Length);
        foreach (ShelterDataLoader.ShelterData gameplayShelter in gameplayShelters)
        {
            Assert.AreEqual(RealQualifiedShelterDataLoader.SourceType, gameplayShelter.sourceType);
            Assert.IsTrue(gameplayShelter.canEnter);
            Assert.IsTrue(gameplayShelter.isOfficialShelter);
            Assert.AreEqual(RealQualifiedShelterDataLoader.PrototypeDebugLayoutCoordinateSystem, gameplayShelter.coordinateSystem);
            Assert.IsFalse(string.IsNullOrWhiteSpace(gameplayShelter.plateauBuildingId));
        }

        GameObject metadataObject = new GameObject("P5D_MetadataSmoke_Test");
        try
        {
            RealQualifiedShelterMetadata metadata = metadataObject.AddComponent<RealQualifiedShelterMetadata>();
            metadata.ApplyRecord(firstSelectable);

            Assert.AreEqual(firstSelectable.gameplayShelterId, metadata.GameplayShelterId);
            Assert.AreEqual(firstSelectable.qualificationStatus, metadata.QualificationStatus);
            Assert.AreEqual(firstSelectable.confidence, metadata.Confidence);
            Assert.AreEqual(firstSelectable.routeDistanceMeters, metadata.RouteDistanceMeters);
            Assert.AreEqual(firstSelectable.estimatedRouteTimeSeconds, metadata.EstimatedRouteTimeSeconds);
            Assert.AreEqual(firstSelectable.attribution, metadata.Attribution);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(metadataObject);
        }
    }

    [Test]
    public void RealQualifiedShelterPromptWarnsButContinuesWhenMetadataIsMissing()
    {
        GameObject shelterObject = new GameObject("P5D_MissingMetadataPrompt_Test");
        try
        {
            BuildingShelter shelter = shelterObject.AddComponent<BuildingShelter>();
            shelter.ApplyShelterData(new ShelterDataLoader.ShelterData
            {
                shelterId = "real_without_metadata",
                shelterName = "Real Qualified Without Metadata",
                shelterRank = "Real",
                isOfficialShelter = true,
                canEnter = true,
                climbTimeSeconds = 10f,
                sourceType = RealQualifiedShelterDataLoader.SourceType,
                facilityType = "qualified_tsunami_evacuation_building"
            });

            LogAssert.Expect(LogType.Warning, new Regex("missing RealQualifiedShelterMetadata"));
            string displayText = shelter.GetDisplayText();

            StringAssert.Contains("Real Qualified Without Metadata", displayText);
            StringAssert.Contains("Enterable", displayText);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(shelterObject);
        }
    }

    [Test]
    public void RealQualifiedLoaderRejectsForbiddenRuntimePaths()
    {
        string assetsData = Path.Combine(Application.dataPath, "Data");
        string forbidden = Path.Combine(
            Application.dataPath,
            "../data_pipeline/processed/qualification/real_chuo_integrated_route_qualification.json");

        LogAssert.Expect(LogType.Warning, new Regex("must use copied static JSON under Assets/Data only"));
        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult result =
            RealQualifiedShelterDataLoader.LoadFromPaths(
                forbidden,
                Path.Combine(assetsData, P5CStaticDataLoader.RouteSampleFileName),
                Path.Combine(assetsData, P5CStaticDataLoader.BuildingQualificationFileName),
                Path.Combine(assetsData, P5CStaticDataLoader.ShelterBuildingMatchesFileName));

        Assert.IsFalse(result.success);
        Assert.IsFalse(result.usesAssetsDataOnly);
        Assert.AreEqual(0, result.records.Length);
    }

    [Test]
    public void SelectabilityRulesMatchP5DStatusPolicy()
    {
        string[] statuses =
        {
            "official_confirmed",
            "official_confirmed_with_review",
            "strong_candidate",
            "weak_candidate",
            "unknown",
            "not_qualified"
        };

        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult result =
            RealQualifiedShelterDataLoader.BuildFromBundle(BuildStatusPolicyBundle(statuses));

        Assert.IsTrue(result.success);
        Assert.AreEqual(statuses.Length, result.records.Length);
        Assert.AreEqual(2, result.selectableCount);

        AssertStatusSelectable(result.records, "official_confirmed", true);
        AssertStatusSelectable(result.records, "official_confirmed_with_review", true);
        AssertStatusSelectable(result.records, "strong_candidate", false);
        AssertStatusSelectable(result.records, "weak_candidate", false);
        AssertStatusSelectable(result.records, "unknown", false);
        AssertStatusSelectable(result.records, "not_qualified", false);

        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord reviewRecord =
            FindByStatus(result.records, "official_confirmed_with_review");
        Assert.IsTrue(reviewRecord.manualReviewNeeded);
        Assert.That(string.Join("\n", reviewRecord.warnings), Does.Contain("manual review"));
    }

    [Test]
    public void RealQualifiedMetadataPreservesRouteAndAttributionFields()
    {
        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult result =
            RealQualifiedShelterDataLoader.LoadFromAssetsData();

        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord first =
            RealQualifiedShelterDataLoader.GetSelectableRecords(result.records)[0];

        Assert.IsFalse(string.IsNullOrWhiteSpace(first.gameplayShelterId));
        Assert.AreEqual(first.officialShelterId, first.gameplayShelterId);
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.displayName));
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.plateauBuildingId));
        Assert.AreEqual("official_confirmed", first.qualificationStatus);
        Assert.AreEqual("high", first.confidence);
        Assert.Greater(first.warnings.Length, 0);
        Assert.IsTrue(first.HasRouteDistanceMeters);
        Assert.IsTrue(first.HasEstimatedRouteTimeSeconds);
        Assert.IsFalse(string.IsNullOrWhiteSpace(first.routeTargetId));
        Assert.AreEqual(P5CStaticDataLoader.EstimatedPrototypeRouteLabel, first.routeDisclaimer);
        Assert.That(first.attribution, Does.Contain("OpenStreetMap"));
        Assert.That(first.attribution, Does.Contain("Open Database License"));
    }

    [Test]
    public void MissingOrAmbiguousMappingFailsSafely()
    {
        P5CStaticDataLoader.P5CDataBundle bundle = BuildMinimalBundle(new[]
        {
            BuildIntegrated("missing_plateau", "official_confirmed", string.Empty, "route_missing_plateau"),
            BuildIntegrated("duplicate_id", "official_confirmed", "plateau_1", "route_duplicate_a"),
            BuildIntegrated("duplicate_id", "official_confirmed", "plateau_1", "route_duplicate_b")
        });

        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult result =
            RealQualifiedShelterDataLoader.BuildFromBundle(bundle);

        Assert.IsFalse(result.success);
        Assert.AreEqual(0, result.selectableCount);

        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord missing =
            FindById(result.records, "missing_plateau");
        Assert.IsFalse(missing.isSelectable);
        Assert.That(missing.nonSelectableReason, Does.Contain("missing plateauBuildingId"));

        foreach (RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record in result.records)
        {
            Assert.IsFalse(record.isSelectable);
            Assert.IsFalse(string.IsNullOrWhiteSpace(record.nonSelectableReason));
        }
    }

    [Test]
    public void RealQualifiedGameplayMappingProducesOnlySelectableRuntimeShelters()
    {
        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult loadResult =
            RealQualifiedShelterDataLoader.LoadFromAssetsData();

        ShelterDataLoader.ShelterData[] mappedShelters =
            ShelterGameplayDataMapper.MapRealQualifiedSheltersToGameplayData(loadResult.records);

        Assert.AreEqual(loadResult.selectableCount, mappedShelters.Length);
        Assert.Greater(mappedShelters.Length, 0);

        foreach (ShelterDataLoader.ShelterData shelter in mappedShelters)
        {
            Assert.AreEqual(RealQualifiedShelterDataLoader.SourceType, shelter.sourceType);
            Assert.AreEqual("qualified_tsunami_evacuation_building", shelter.facilityType);
            Assert.IsTrue(shelter.canEnter);
            Assert.IsTrue(shelter.isOfficialShelter);
            Assert.AreEqual(RealQualifiedShelterDataLoader.PrototypeDebugLayoutCoordinateSystem, shelter.coordinateSystem);
            Assert.IsFalse(string.IsNullOrWhiteSpace(shelter.plateauBuildingId));
        }
    }

    [Test]
    public void RealQualifiedFeedbackPreservesDisclaimerAndOdbLAttribution()
    {
        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult loadResult =
            RealQualifiedShelterDataLoader.LoadFromAssetsData();
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord first =
            RealQualifiedShelterDataLoader.GetSelectableRecords(loadResult.records)[0];

        string feedback = RealQualifiedShelterFeedbackFormatter.BuildFromLoadResult(
            loadResult,
            first.gameplayShelterId,
            true);

        StringAssert.Contains("Real qualified shelter:", feedback);
        StringAssert.Contains("Qualification:", feedback);
        StringAssert.Contains("Manual review:", feedback);
        StringAssert.Contains("Estimated prototype route:", feedback);
        StringAssert.Contains(P5CStaticDataLoader.EstimatedPrototypeRouteLabel, feedback);
        StringAssert.Contains("OSM/ODbL attribution:", feedback);
        StringAssert.Contains("OpenStreetMap", feedback);
        StringAssert.Contains("Open Database License", feedback);
        Assert.LessOrEqual(Regex.Matches(feedback, "Warning:").Count, 2);
    }

    [Test]
    public void RealQualifiedFeedbackFailsClosedForMissingAndMalformedInputs()
    {
        Assert.AreEqual(
            RealQualifiedShelterDataLoader.UnavailableMessage,
            RealQualifiedShelterFeedbackFormatter.BuildForShelterId(null));
        Assert.AreEqual(
            RealQualifiedShelterDataLoader.UnavailableMessage,
            RealQualifiedShelterFeedbackFormatter.BuildFromLoadResult(null, string.Empty, true));
        Assert.AreEqual(
            string.Empty,
            RealQualifiedShelterFeedbackFormatter.BuildFromLoadResult(null, string.Empty, false));

        var record = new RealQualifiedShelterDataLoader.RealQualifiedShelterRecord
        {
            gameplayShelterId = "malformed_optional",
            displayName = "Malformed Optional Record",
            qualificationStatus = null,
            confidence = null,
            warnings = new[] { null, string.Empty, " optional warning " },
            routeDistanceMeters = float.NaN,
            estimatedRouteTimeSeconds = float.PositiveInfinity,
            attribution = null
        };
        var loadResult = new RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult
        {
            records = new[] { record }
        };

        string feedback = null;
        Assert.DoesNotThrow(() =>
        {
            feedback = RealQualifiedShelterFeedbackFormatter.BuildFromLoadResult(
                loadResult,
                record.gameplayShelterId,
                true);
        });

        StringAssert.Contains("Qualification: unknown / unknown", feedback);
        StringAssert.Contains("distance unavailable", feedback);
        StringAssert.Contains("estimated time unavailable", feedback);
        StringAssert.Contains("OSM/ODbL attribution:", feedback);
    }

    [Test]
    public void RouteGeometryParserHandlesActualLonLatLineString()
    {
        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.RouteSampleRecord> routes =
            P5CStaticDataLoader.LoadRouteSamplesFromPath(AssetsDataPath(P5CStaticDataLoader.RouteSampleFileName));

        Assert.IsTrue(routes.success);
        P5CStaticDataLoader.RouteSampleRecord first = routes.records[0];

        Assert.AreEqual("LineString", first.geometry.type);
        Assert.AreEqual("EPSG:4326", first.geometry.coordinateReferenceSystem);
        Assert.Greater(first.geometry.coordinates.Length, 2);
        Assert.Greater(first.geometry.coordinates[0].x, 139f);
        Assert.Less(first.geometry.coordinates[0].x, 140f);
        Assert.Greater(first.geometry.coordinates[0].y, 35f);
        Assert.Less(first.geometry.coordinates[0].y, 36f);
    }

    [Test]
    public void RouteTransformValidationRejectsUnsafeWgs84Geometry()
    {
        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.RouteSampleRecord> routes =
            P5CStaticDataLoader.LoadRouteSamplesFromPath(AssetsDataPath(P5CStaticDataLoader.RouteSampleFileName));

        P5DRoutePreviewTransformValidator.ValidationResult validation =
            P5DRoutePreviewTransformValidator.ValidateForSelectedRoutePreview(
                routes.records[0],
                Vector3.zero,
                new Vector3(8f, 0f, -14f));

        Assert.IsFalse(validation.canRender);
        Assert.Greater(validation.horizontalSpanMeters, 0f);
        Assert.That(validation.reason, Does.Contain("no verified WGS84-to-Unity/PLATEAU transform exists"));
    }

    [Test]
    public void RouteTransformValidationRejectsInvalidWgs84OrderAndBounds()
    {
        P5CStaticDataLoader.RouteSampleRecord swapped = BuildWgs84Route(
            "swapped_wgs84",
            new[]
            {
                new Vector2(35.67157f, 139.76523f),
                new Vector2(35.67205f, 139.76575f)
            });

        P5DRoutePreviewTransformValidator.ValidationResult swappedValidation =
            P5DRoutePreviewTransformValidator.ValidateForSelectedRoutePreview(
                swapped,
                Vector3.zero,
                new Vector3(8f, 0f, -14f));

        Assert.IsFalse(swappedValidation.canRender);
        Assert.That(swappedValidation.reason, Does.Contain("latitude/longitude order"));

        P5CStaticDataLoader.RouteSampleRecord farAway = BuildWgs84Route(
            "far_wgs84",
            new[]
            {
                new Vector2(130f, 35f),
                new Vector2(130.01f, 35.01f)
            });

        P5DRoutePreviewTransformValidator.ValidationResult farAwayValidation =
            P5DRoutePreviewTransformValidator.ValidateForSelectedRoutePreview(
                farAway,
                Vector3.zero,
                new Vector3(8f, 0f, -14f));

        Assert.IsFalse(farAwayValidation.canRender);
        Assert.That(farAwayValidation.reason, Does.Contain("outside the broad Chuo WGS84 validation bounds"));
    }

    [Test]
    public void RouteTransformValidationAcceptsOnlySafeUnityDebugGeometry()
    {
        P5CStaticDataLoader.RouteSampleRecord safeRoute = BuildUnityDebugRoute(
            "debug_safe",
            new[]
            {
                new Vector2(0f, 0f),
                new Vector2(4f, -5f),
                new Vector2(8f, -14f)
            });

        P5DRoutePreviewTransformValidator.ValidationResult safeValidation =
            P5DRoutePreviewTransformValidator.ValidateForSelectedRoutePreview(
                safeRoute,
                Vector3.zero,
                new Vector3(8f, 0f, -14f));

        Assert.IsTrue(safeValidation.canRender);
        Assert.AreEqual(3, safeValidation.localPositions.Length);

        P5CStaticDataLoader.RouteSampleRecord collapsed = BuildUnityDebugRoute(
            "debug_collapsed",
            new[] { Vector2.zero, Vector2.zero });

        P5DRoutePreviewTransformValidator.ValidationResult collapsedValidation =
            P5DRoutePreviewTransformValidator.ValidateForSelectedRoutePreview(
                collapsed,
                Vector3.zero,
                Vector3.zero);

        Assert.IsFalse(collapsedValidation.canRender);
        Assert.That(collapsedValidation.reason, Does.Contain("collapsed"));

        P5CStaticDataLoader.RouteSampleRecord reversed = BuildUnityDebugRoute(
            "debug_reversed",
            new[]
            {
                new Vector2(8f, -14f),
                new Vector2(0f, 0f)
            });

        P5DRoutePreviewTransformValidator.ValidationResult reversedValidation =
            P5DRoutePreviewTransformValidator.ValidateForSelectedRoutePreview(
                reversed,
                Vector3.zero,
                new Vector3(8f, 0f, -14f),
                1f,
                100f);

        Assert.IsFalse(reversedValidation.canRender);
        Assert.That(reversedValidation.reason, Does.Contain("route start"));
    }

    [Test]
    public void RoutePreviewValidationFailureKeepsRenderingDisabledForCurrentData()
    {
        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult loadResult =
            RealQualifiedShelterDataLoader.LoadFromAssetsData();
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord first =
            RealQualifiedShelterDataLoader.GetSelectableRecords(loadResult.records)[0];

        P5DRoutePreviewTransformValidator.ValidationResult validation =
            P5DRoutePreviewTransformValidator.ValidateForSelectedRoutePreview(
                first.routeSample,
                Vector3.zero,
                ShelterGameplayDataMapper.CreateDeterministicFallbackLayout(0).ToVector3());

        Assert.IsFalse(validation.canRender);
        Assert.That(validation.reason, Does.Contain("no verified WGS84-to-Unity/PLATEAU transform exists"));
    }

    private static P5CStaticDataLoader.P5CDataBundle BuildStatusPolicyBundle(string[] statuses)
    {
        var integrated = new P5CStaticDataLoader.IntegratedRouteQualificationRecord[statuses.Length];
        for (int i = 0; i < statuses.Length; i++)
        {
            integrated[i] = BuildIntegrated($"status_{i}", statuses[i], $"plateau_{i}", $"route_{i}");
        }

        return BuildMinimalBundle(integrated);
    }

    private static P5CStaticDataLoader.P5CDataBundle BuildMinimalBundle(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord[] integrated)
    {
        var routes = new P5CStaticDataLoader.RouteSampleRecord[integrated.Length];
        var buildings = new P5CStaticDataLoader.BuildingQualificationRecord[integrated.Length];
        var matches = new P5CStaticDataLoader.ShelterBuildingMatchRecord[integrated.Length];

        for (int i = 0; i < integrated.Length; i++)
        {
            P5CStaticDataLoader.IntegratedRouteQualificationRecord record = integrated[i];
            routes[i] = BuildUnityDebugRoute(record.nearestRouteId, new[] { Vector2.zero, new Vector2(8f, -14f) });
            routes[i].shelterId = record.shelterId;
            routes[i].plateauBuildingId = record.plateauBuildingId;
            buildings[i] = new P5CStaticDataLoader.BuildingQualificationRecord
            {
                shelterId = record.shelterId,
                shelterName = record.shelterName,
                plateauBuildingId = record.plateauBuildingId,
                qualificationStatus = record.qualificationStatus,
                confidence = "high",
                safeFloor = -1,
                capacity = -1
            };
            matches[i] = new P5CStaticDataLoader.ShelterBuildingMatchRecord
            {
                shelterId = record.shelterId,
                shelterName = record.shelterName,
                plateauBuildingId = record.plateauBuildingId,
                confidence = "high"
            };
        }

        return new P5CStaticDataLoader.P5CDataBundle
        {
            integratedRouteQualifications =
                BuildLoadResult("inline integrated", integrated),
            routeSamples = BuildLoadResult("inline routes", routes),
            buildingQualifications = BuildLoadResult("inline buildings", buildings),
            shelterBuildingMatches = BuildLoadResult("inline matches", matches)
        };
    }

    private static P5CStaticDataLoader.IntegratedRouteQualificationRecord BuildIntegrated(
        string shelterId,
        string status,
        string plateauBuildingId,
        string routeId)
    {
        return new P5CStaticDataLoader.IntegratedRouteQualificationRecord
        {
            shelterId = shelterId,
            shelterName = $"Shelter {shelterId}",
            plateauBuildingId = plateauBuildingId,
            qualificationStatus = status,
            confidence = "high",
            manualReviewNeeded = false,
            warnings = new[] { "OSM route is an estimated prototype pedestrian route, not an official evacuation route" },
            nearestRouteId = routeId,
            nearestRouteDistanceMeters = 123.4f,
            estimatedTravelTimeSeconds = 56.7f,
            routeType = "estimated_pedestrian_route",
            isOfficialEvacuationRoute = false
        };
    }

    private static P5CStaticDataLoader.RouteSampleRecord BuildUnityDebugRoute(string routeId, Vector2[] coordinates)
    {
        return new P5CStaticDataLoader.RouteSampleRecord
        {
            routeId = routeId,
            routeAvailability = "available",
            routeDistanceMeters = 123.4f,
            estimatedTravelTimeSeconds = 56.7f,
            routeType = "estimated_pedestrian_route",
            isOfficialEvacuationRoute = false,
            geometry = new P5CStaticDataLoader.RouteGeometryRecord
            {
                type = "LineString",
                coordinates = coordinates,
                coordinateReferenceSystem = P5DRoutePreviewTransformValidator.UnityDebugCoordinateSystem
            }
        };
    }

    private static P5CStaticDataLoader.RouteSampleRecord BuildWgs84Route(string routeId, Vector2[] coordinates)
    {
        return new P5CStaticDataLoader.RouteSampleRecord
        {
            routeId = routeId,
            routeAvailability = "available",
            routeDistanceMeters = 123.4f,
            estimatedTravelTimeSeconds = 56.7f,
            routeType = "estimated_pedestrian_route",
            isOfficialEvacuationRoute = false,
            geometry = new P5CStaticDataLoader.RouteGeometryRecord
            {
                type = "LineString",
                coordinates = coordinates,
                coordinateReferenceSystem = P5CStaticDataLoader.Wgs84CoordinateSystem
            }
        };
    }

    private static P5CStaticDataLoader.LoadResult<TRecord> BuildLoadResult<TRecord>(
        string sourcePath,
        TRecord[] records)
    {
        return new P5CStaticDataLoader.LoadResult<TRecord>
        {
            success = records != null && records.Length > 0,
            sourcePath = sourcePath,
            datasetId = "inline_p5d_fixture",
            generatedAt = "2026-05-20T00:00:00Z",
            coordinateReferenceSystem = "UNITY_DEBUG",
            osmAttribution = "Route network data from OpenStreetMap contributors; use under the Open Database License.",
            records = records
        };
    }

    private static void AssertStatusSelectable(
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord[] records,
        string status,
        bool expectedSelectable)
    {
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record = FindByStatus(records, status);

        Assert.NotNull(record, status);
        Assert.AreEqual(expectedSelectable, record.isSelectable, status);
    }

    private static RealQualifiedShelterDataLoader.RealQualifiedShelterRecord FindByStatus(
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord[] records,
        string status)
    {
        foreach (RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record in records)
        {
            if (record != null && record.qualificationStatus == status)
            {
                return record;
            }
        }

        return null;
    }

    private static RealQualifiedShelterDataLoader.RealQualifiedShelterRecord FindById(
        RealQualifiedShelterDataLoader.RealQualifiedShelterRecord[] records,
        string shelterId)
    {
        foreach (RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record in records)
        {
            if (record != null && record.gameplayShelterId == shelterId)
            {
                return record;
            }
        }

        return null;
    }

    private static string AssetsDataPath(string fileName)
    {
        return Path.Combine(Application.dataPath, "Data", fileName);
    }

    private static string Normalize(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/');
    }
}
