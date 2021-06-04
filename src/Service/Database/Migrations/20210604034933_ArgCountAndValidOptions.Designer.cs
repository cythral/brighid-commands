﻿// <auto-generated />
using System;
using Brighid.Commands.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Brighid.Commands.Database
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20210604034933_ArgCountAndValidOptions")]
    partial class ArgCountAndValidOptions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.6");

            modelBuilder.Entity("Brighid.Commands.Commands.Command", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("binary(16)");

                    b.Property<uint>("ArgCount")
                        .HasColumnType("int unsigned");

                    b.Property<string>("AssemblyName")
                        .HasColumnType("longtext");

                    b.Property<string>("Checksum")
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("DownloadURL")
                        .HasColumnType("longtext");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("TypeName")
                        .HasColumnType("longtext");

                    b.Property<string>("ValidOptions")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Commands");
                });
#pragma warning restore 612, 618
        }
    }
}
