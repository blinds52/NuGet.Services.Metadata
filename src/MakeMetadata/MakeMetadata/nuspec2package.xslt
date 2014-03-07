﻿<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:nuget="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd"
  xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
  xmlns:ng="http://nuget.org/schema#"
  version="1.0">

  <xsl:param name="base" />

  <xsl:variable name="lowercase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

  <xsl:template match="/nuget:package/nuget:metadata">
    <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
      <ng:Package>

        <xsl:variable name="path" select="concat(nuget:id, '/', nuget:version)" />

        <xsl:attribute name="rdf:about">
          <xsl:value-of select="translate(concat($base, $path), $uppercase, $lowercase)"/>
        </xsl:attribute>
        
        <ng:registration>
          <rdf:Description>
            <xsl:attribute name="rdf:about">
              <xsl:value-of select="translate(concat($base, nuget:id), $uppercase, $lowercase)"/>
            </xsl:attribute>
          </rdf:Description>
        </ng:registration>
        
        <xsl:for-each select="*">
          <xsl:choose>
            <xsl:when test="self::nuget:dependencies">
              <xsl:apply-templates select="nuget:group">
                <xsl:with-param name="parent" select="$path" />
              </xsl:apply-templates>
              <xsl:apply-templates select="nuget:dependency">
                <xsl:with-param name="parent" select="$path" />
              </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
              <xsl:element name="{concat('ng:', local-name())}">
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      
      </ng:Package>
    </rdf:RDF>
  </xsl:template>
 
  <xsl:template match="nuget:group">
    <xsl:param name="parent" />
    <ng:group>
      <rdf:Description>

        <xsl:variable name="dep">
          <xsl:choose>
            <xsl:when test="@targetFramework">
              <xsl:value-of select="concat('#dep_', @targetFramework)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'#dep'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="path" select="concat($parent, $dep)" />

        <xsl:attribute name="rdf:about">
          <xsl:value-of select="translate(concat($base, $path), uppercase, $lowercase)"/>
        </xsl:attribute>

        <xsl:if test="@targetFramework">
          <ng:targetFramework>
            <xsl:value-of select="@targetFramework"/>
          </ng:targetFramework>
        </xsl:if>
        
        <xsl:apply-templates select="nuget:dependency">
          <xsl:with-param name="parent" select="$path" />
        </xsl:apply-templates>

      </rdf:Description>
    </ng:group>
  </xsl:template>

  <xsl:template match="nuget:dependency">
    <xsl:param name="parent" />
    <ng:dependency>
      <rdf:Description>

        <xsl:variable name="path" select="concat($parent, '_', @id)" />

        <xsl:attribute name="rdf:about">
          <xsl:value-of select="translate(concat($base, $path), $uppercase, $lowercase)"/>
        </xsl:attribute>

        <ng:id>
          <xsl:value-of select="@id"/>
        </ng:id>

        <ng:range>
          <xsl:value-of select="@version"/>
        </ng:range>

        <ng:registration>
          <rdf:Description>
            <xsl:attribute name="rdf:about">
              <xsl:value-of select="translate(concat($base, @id), $uppercase, $lowercase)"/>
            </xsl:attribute>
          </rdf:Description>
        </ng:registration>

      </rdf:Description>
    </ng:dependency>
  </xsl:template>

</xsl:stylesheet>