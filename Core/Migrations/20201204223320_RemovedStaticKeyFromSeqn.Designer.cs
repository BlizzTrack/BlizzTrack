﻿// <auto-generated />
using System;
using BNetLib.Models;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20201204223320_RemovedStaticKeyFromSeqn")]
    partial class RemovedStaticKeyFromSeqn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasPostgresExtension("pg_trgm")
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.BGDL[]>", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<int>("Seqn")
                        .HasColumnType("integer");

                    b.Property<BGDL[]>("Content")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Code", "Seqn");

                    b.ToTable("bgdl");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.CDN[]>", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<int>("Seqn")
                        .HasColumnType("integer");

                    b.Property<CDN[]>("Content")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Code", "Seqn");

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

                    b.Property<Summary[]>("Content")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Seqn");

                    b.ToTable("summary");
                });

            modelBuilder.Entity("Core.Models.Manifest<BNetLib.Models.Versions[]>", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<int>("Seqn")
                        .HasColumnType("integer");

                    b.Property<Versions[]>("Content")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Indexed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Code", "Seqn");

                    b.ToTable("versions");
                });
#pragma warning restore 612, 618
        }
    }
}
