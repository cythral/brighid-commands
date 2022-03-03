﻿// <auto-generated />
using System;
using Brighid.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Brighid.Commands.Database
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220303035345_AddCommandVersions")]
    partial class AddCommandVersions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Brighid.Commands.Service.Command", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("binary(16)");

                    b.Property<uint>("ArgCount")
                        .HasColumnType("int unsigned");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("EmbeddedLocation")
                        .HasColumnType("longtext");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(95)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("binary(16)");

                    b.Property<string>("Parameters")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RequiredRole")
                        .HasColumnType("longtext");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("ValidOptions")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Version")
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
