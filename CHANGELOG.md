﻿# Change Log
All notable changes to OpenRasta will be documented in this file.
OpenRasta adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### Added
 - A new pipeline with "double tap" semantics, which makes most of the old pipeline
   look like an old picture, keen memories but visibly dated.
 - Things don't always happen when we want them too, so everything has been made
   async. This includes async contributors with a new `NotifyAsync`, async
   handlers (just return `Task` or `Task<T>`), codecs with `IMediaTypeReaderAsync`
   and `IMediaTypeWriterAsync`, and async interceptor with `IOperationInterceptorAsync`

### Changed
 - Cool kids have moved on, so we follow. .net 4.6.1 is now a minimum.
 - The AppVeyor build is now under the OpenRasta organisation. We were not
   really organised before, so we (well, @holytshirt) sorted it out, and we feel
   much cleaner.
 - `IPipeline` can now only be used in the contributor configuration pahse
   `IEnumerable`.
 - Methods on handlers returning `void` or `Task` will now return a 202 accepted
   instead of a 204 no content. If we can't know the semantics we shouldn't give
   any better guarantees.
 - Operations that were not ready for invocation would sometime fail. Now,
   either we respond with a 400 when we can't match the request to the response,
   or a `500` if we can't chose the correct method because the call was ambiguous.
 - I think errors should be given as much attention as non-errors, that's why we
   always content negotiate those errors, so we can exceptionally render exceptional
   exceptions using whatever codec you want to use, which is great for your APIs,
   but a bit less for your tests. You can now chose if you want that functionality
   or if you'd rather let exceptions bubble up, so your server can show their default
   error page or whatever.
 - ⌚️ is relative, apparently. But `RedirectLocation` wasn't. So we allowed relative
   URIs. 


### Deprecated
 - All members bar `Notify` on `IPipeline`. Because really, why would you ever
   want to know anything about the pipeline.
 - `IOperationHydrator` is no more. If you have your own, we'll respect that,
   but we'd rather you used our new and shiny `IRequestEntityReader`.
 - PipelineContinuation is moslty gone, it wasn't really meant to be used externally.
   We made `Abort` obsolete, you should instead throw an exception from the
   contributor, for those moments exceptional stuff happens. Even `Finished` was
   a bit of a lie. You reached the end of the line or you didn't, and that's all
   that ever really mattered. The responsible developer is very sorry and promised
   he will never lie again.

### Removed

### Fixed
 - When padding error messages in HTML for IE clients, we would send an
   incorrect Length, and no one wants that, so we made it accurate and bigger.
 - We would try and pad the result even if the response stream is not seekable,
   now we check correctly.

### Security

## [2.5.1050] - 2016-12-15
### Fixed
 - When we removed the authentication support from the out of the box experience,
   we forgot that some people took dependencies on this essential step of the
   pipeline. We apologised profusely and reintroduced
    `KnownStages.IAuthentication` promptly.

## [2.5.1023] - 2016-08-24
### Changed
 - Made uri to handler method parameters mapping case-insensitive.

## [2.5.63] - 2016-03-22
### Fixed
 - Removes a resource leak in the multipart codec

### Changed
 - The error message when an operation was not ready to be executed was a bit
   terse and not very helpful. We made it more helpful.

## [2.5.59] - 2015-10-23

### Changed
 - Improved Content-Disposition header parsing
 - IHttpEntity is now disposable, and disposed properly

## [2.5.54] - 2015-08-11
### Fixed
 - Concurrency issue in SurrogateBuilderProvider

## [2.5.53] - 2015-03-18
### Added
 - The pipeline can use a new topological sort instead of a weighted sort
   with potentially better support for more complex pipelines. Can be added
   through the container with an `ICallGraphGenerator`

### Fixed
 - Query strings now get + processing correctly

## [2.5.48] - 2015-02-23
### Added
 - forms now support nullable Guids

## [2.5.46] - 2015-06-02
### Fixed
 - Object binders on parameters had been inadventantly removed when
   migrating to GitHub. They now work as expected.
 - Malformed Guids would cause 500. They now don't match.


## [2.5.42] - 2015-01-20
### Fixed
 - A malformed Accept header would cause a 500, it now ignores malformed
   values, and returns a 400 if all values are invalid.

### Added
 - OperationResult.Accepted has been added


## [2.5.34] - 2014-12-16
### Changed
 - The Codec response selection would always set the pipeline as finished,
   preventing contributors from running afterwards. The contributor was changed
   to only set the pipeline as continue.

## [2.5.31] - 2014-10-08
### Fixed
 - error HTML and CSS files were incorrectly shipped in NuGet packages

## [2.5.28] - 2014-10-08
### Fixed
 - Race condition in HttpListenerHost on Dispose

## [2.5.23] - 2014-09-03

This release has no new features or fixes.

## [2.5.21] - 2014-09-03
### Added
 - OpenRasta.Testing now contains reusable context base classes

## [2.5.13] - 2014-08-21
### Added
 - `HttpMethod` now has a `Patch`

## [2.5.7] - 2014-06-26

Previous OpenRasta versions are not included as they were on OpenWrap.

### Changed
 - OpenRasta is now on nuget


