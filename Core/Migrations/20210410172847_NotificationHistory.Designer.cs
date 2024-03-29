﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Text.Json;
using BNetLib.Ribbit.Models;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20210410172847_NotificationHistory")]
    partial class NotificationHistory
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasPostgresExtension("pg_trgm")
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.4");

            modelBuilder.Entity("Core.Models.Assets", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<JsonDocument>("Metadata")
                        .HasColumnType("jsonb");

                    b.Property<string>("url")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Assets");
                });

            modelBuilder.Entity("Core.Models.Catalog", b =>
                {
                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<bool>("Activision")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<List<CatalogInstall>>("Installs")
                        .HasColumnType("jsonb");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<JsonDocument>("Payload")
                        .HasColumnType("jsonb");

                    b.Property<string>("ProperName")
                        .HasColumnType("text");

                    b.Property<string>("Raw")
                        .HasColumnType("text");

                    b.Property<Dictionary<string, string>>("Translations")
                        .HasColumnType("jsonb");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Hash");

                    b.ToTable("Catalogs");
                });

            modelBuilder.Entity("Core.Models.GameChildren", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("ParentCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Slug")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ParentCode");

                    b.ToTable("GameChildren");
                });

            modelBuilder.Entity("Core.Models.GameCompany", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("GameCompanies");
                });

            modelBuilder.Entity("Core.Models.GameConfig", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<ConfigItems>("Config")
                        .HasColumnType("jsonb");

                    b.Property<List<Icons>>("Logos")
                        .HasColumnType("jsonb");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("ServiceURL")
                        .HasColumnType("text");

                    b.Property<string>("Website")
                        .HasColumnType("text");

                    b.HasKey("Code");

                    b.ToTable("game_configs");
                });

            modelBuilder.Entity("Core.Models.GameParents", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("About")
                        .HasColumnType("text");

                    b.Property<List<string>>("ChildrenOverride")
                        .HasColumnType("text[]");

                    b.Property<List<Icons>>("Logos")
                        .HasColumnType("jsonb");

                    b.Property<string>("ManifestID")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("OwnerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(3);

                    b.Property<List<string>>("PatchNoteAreas")
                        .HasColumnType("text[]");

                    b.Property<string>("PatchNoteCode")
                        .HasColumnType("text");

                    b.Property<string>("PatchNoteTool")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("legacy");

                    b.Property<string>("Slug")
                        .HasColumnType("text");

                    b.Property<bool?>("Visible")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<string>("Website")
                        .HasColumnType("text");

                    b.HasKey("Code");

                    b.HasIndex("OwnerId");

                    b.ToTable("GameParents");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.BGDL[]>", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<int>("Seqn")
                        .HasColumnType("integer");

                    b.Property<string>("ConfigId")
                        .HasColumnType("text");

                    b.Property<BGDL[]>("Content")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("Parent")
                        .HasColumnType("integer");

                    b.Property<string>("Raw")
                        .HasColumnType("text");

                    b.HasKey("Code", "Seqn");

                    b.HasIndex("ConfigId");

                    b.ToTable("bgdl");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.CDN[]>", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<int>("Seqn")
                        .HasColumnType("integer");

                    b.Property<string>("ConfigId")
                        .HasColumnType("text");

                    b.Property<CDN[]>("Content")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("Parent")
                        .HasColumnType("integer");

                    b.Property<string>("Raw")
                        .HasColumnType("text");

                    b.HasKey("Code", "Seqn");

                    b.HasIndex("ConfigId");

                    b.ToTable("cdns");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.Summary[]>", b =>
                {
                    b.Property<int>("Seqn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("ConfigId")
                        .HasColumnType("text");

                    b.Property<Summary[]>("Content")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Raw")
                        .HasColumnType("text");

                    b.HasKey("Seqn");

                    b.ToTable("summary");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.Versions[]>", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<int>("Seqn")
                        .HasColumnType("integer");

                    b.Property<string>("ConfigId")
                        .HasColumnType("text");

                    b.Property<Versions[]>("Content")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("Parent")
                        .HasColumnType("integer");

                    b.Property<string>("Raw")
                        .HasColumnType("text");

                    b.HasKey("Code", "Seqn");

                    b.HasIndex("ConfigId");

                    b.ToTable("versions");
                });

            modelBuilder.Entity("Core.Models.NotificationHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("File")
                        .HasColumnType("text");

                    b.Property<int>("NotificationType")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Sent")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Seqn")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("NotificationHistories");
                });

            modelBuilder.Entity("Core.Models.PatchNote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<JsonDocument>("Body")
                        .HasColumnType("jsonb");

                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Language")
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("PatchNotes");
                });

            modelBuilder.Entity("Core.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("AccessToken")
                        .HasColumnType("text");

                    b.Property<string>("Avatar")
                        .HasColumnType("text");

                    b.Property<string>("BattleTag")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Core.Models.GameChildren", b =>
                {
                    b.HasOne("Core.Models.GameParents", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Core.Models.GameConfig", b =>
                {
                    b.HasOne("Core.Models.GameChildren", "Owner")
                        .WithOne("GameConfig")
                        .HasForeignKey("Core.Models.GameConfig", "Code")
                        .HasPrincipalKey("Core.Models.GameChildren", "Code")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Core.Models.GameParents", b =>
                {
                    b.HasOne("Core.Models.GameCompany", "Owner")
                        .WithMany("Parents")
                        .HasForeignKey("OwnerId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.BGDL[]>", b =>
                {
                    b.HasOne("Core.Models.GameConfig", "Config")
                        .WithOne()
                        .HasForeignKey("Core.Models.Manifest<BNetLib.Models.BGDL[]>", "ConfigId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Config");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.CDN[]>", b =>
                {
                    b.HasOne("Core.Models.GameConfig", "Config")
                        .WithOne()
                        .HasForeignKey("Core.Models.Manifest<BNetLib.Models.CDN[]>", "ConfigId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Config");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.Versions[]>", b =>
                {
                    b.HasOne("Core.Models.GameConfig", "Config")
                        .WithOne()
                        .HasForeignKey("Core.Models.Manifest<BNetLib.Models.Versions[]>", "ConfigId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Config");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Core.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Core.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Core.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Core.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Core.Models.GameChildren", b =>
                {
                    b.Navigation("GameConfig");
                });

            modelBuilder.Entity("Core.Models.GameCompany", b =>
                {
                    b.Navigation("Parents");
                });

            modelBuilder.Entity("Core.Models.GameParents", b =>
                {
                    b.Navigation("Children");
                });
#pragma warning restore 612, 618
        }
    }
}
