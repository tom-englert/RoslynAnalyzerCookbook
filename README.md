# Roslyn Analyzer Cookbook

This repository is a demo project showing how create custom Roslyn analyzers. 

It focuses on how to add custom analyzers to a solution, which enforce custom rules and design patterns that apply only to a specific solution, without the need to create a package or extension.

However most of the topics also apply when generating an analyzer package or extension, and will be also useful in this scenarios.

> Most of the chapters correspond to a commit of the related code, so one can follow this tutorial step by step.

## Points of interest

- Improved test framework: Sanitized and simplified test verifiers, with up to date defaults, using extension methods to customize for individual tests using fluent notation.
- Shows how to reference NuGet packages in the test code, automated by MSBuild.
- Integration of the analyzers into the solution without the need to create a package or install a Visual Studio extension.

## Use case #1

Enforce that every property that has a `[Text]` attribute also has a `[Description]` attribute, by showing a warning,
so e.g. a basic user documentation can be generated automatically for dedicated properties using reflection.

> In real life the will be probably a more specific attribute than a simple `[Text]`, this is just used to make this sample more universal.

