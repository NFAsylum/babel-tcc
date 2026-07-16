plugins {
    id("java")
    id("org.jetbrains.kotlin.jvm") version "1.9.20"
    id("org.jetbrains.intellij") version "1.16.0"
}

group = "com.nfasylum.babel"
version = "0.1.0"

repositories {
    mavenCentral()
}

dependencies {
    testImplementation("junit:junit:4.13.2")
    testImplementation("org.jetbrains.kotlin:kotlin-test-junit")
}

// IntelliJ Platform SDK. Type "IC" (IntelliJ Community) is used for local
// development; the same plugin bytecode loads in Rider and the other
// JetBrains IDEs that share the platform (see plugin.xml <depends>).
intellij {
    version.set("2023.1")
    type.set("IC")
    plugins.set(listOf())
}

// --- Bundling: ship Core.Host + translations inside the plugin (zero manual config) ---
val babelRepoRoot = rootDir.resolve("../../..").canonicalFile
val coreHostProject = babelRepoRoot.resolve("packages/core/MultiLingualCode.Core.Host")
val translationsRepo = babelRepoRoot.parentFile.resolve("babel-tcc-translations")
val coreHostBundleDir = layout.buildDirectory.dir("babel-bundle/core-host")
val translationsBundleDir = layout.buildDirectory.dir("babel-bundle/translations")

tasks.register<Exec>("buildCoreHost") {
    description = "dotnet publish the C# Core.Host binary into the plugin bundle staging dir"
    group = "babel"
    workingDir = coreHostProject
    commandLine(
        "dotnet", "publish",
        "-c", "Release",
        "--no-self-contained",
        "-o", coreHostBundleDir.get().asFile.absolutePath,
    )
    inputs.dir(coreHostProject.resolve("."))
    outputs.dir(coreHostBundleDir)
    doFirst {
        require(coreHostProject.exists()) {
            "Core.Host project not found at $coreHostProject — check repo layout"
        }
    }
}

tasks.register<Copy>("bundleTranslations") {
    description = "Copy babel-tcc-translations JSON tables into the plugin bundle staging dir"
    group = "babel"
    from(translationsRepo) {
        exclude(".git/**", "README*", "LICENSE*", ".gitignore")
    }
    into(translationsBundleDir)
    doFirst {
        require(translationsRepo.exists()) {
            "babel-tcc-translations not found at $translationsRepo — clone it alongside babel-tcc"
        }
    }
}

tasks {
    withType<JavaCompile> {
        sourceCompatibility = "17"
        targetCompatibility = "17"
    }
    withType<org.jetbrains.kotlin.gradle.tasks.KotlinCompile> {
        kotlinOptions.jvmTarget = "17"
    }
    patchPluginXml {
        // Open-ended range: works on IntelliJ 2023.1+ and Rider 251+ (validated by Marco).
        sinceBuild.set("231")
        untilBuild.set(provider { null })
    }
    prepareSandbox {
        dependsOn("buildCoreHost", "bundleTranslations")
        from(coreHostBundleDir) { into("${intellij.pluginName.get()}/core-host") }
        from(translationsBundleDir) { into("${intellij.pluginName.get()}/translations") }
    }
    prepareTestingSandbox {
        dependsOn("buildCoreHost", "bundleTranslations")
        from(coreHostBundleDir) { into("${intellij.pluginName.get()}/core-host") }
        from(translationsBundleDir) { into("${intellij.pluginName.get()}/translations") }
    }
}
