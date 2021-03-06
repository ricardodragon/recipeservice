﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using recipeservice.Data;
using recipeservice.Model;
using System;

namespace recipeservice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180323190848_AddedProductionOrderType")]
    partial class AddedProductionOrderType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("recipeservice.Model.AdditionalInformation", b =>
                {
                    b.Property<int>("additionalInformationId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Information")
                        .HasMaxLength(50);

                    b.Property<string>("Value")
                        .HasMaxLength(50);

                    b.Property<int?>("productId");

                    b.HasKey("additionalInformationId");

                    b.HasIndex("productId");

                    b.ToTable("AdditionalInformations");
                });

            modelBuilder.Entity("recipeservice.Model.ExtraAttibruteType", b =>
                {
                    b.Property<int>("extraAttibruteTypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("extraAttibruteTypeName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("extraAttibruteTypeId");

                    b.ToTable("ExtraAttibruteTypes");
                });

            modelBuilder.Entity("recipeservice.Model.Phase", b =>
                {
                    b.Property<int>("phaseId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("phaseCode")
                        .HasMaxLength(100);

                    b.Property<string>("phaseName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("predecessorPhaseId");

                    b.Property<int[]>("sucessorPhasesIds");

                    b.HasKey("phaseId");

                    b.ToTable("Phases");
                });

            modelBuilder.Entity("recipeservice.Model.PhaseParameter", b =>
                {
                    b.Property<int>("phaseParameterId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("phaseId");

                    b.Property<string>("setupValue")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("tagId");

                    b.HasKey("phaseParameterId");

                    b.HasIndex("phaseId");

                    b.ToTable("PhaseParameters");
                });

            modelBuilder.Entity("recipeservice.Model.PhaseProduct", b =>
                {
                    b.Property<int>("phaseProductId")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("maxValue");

                    b.Property<string>("measurementUnit");

                    b.Property<double>("minValue");

                    b.Property<int?>("phaseId");

                    b.Property<int>("phaseProductType");

                    b.Property<int>("productId");

                    b.HasKey("phaseProductId");

                    b.HasIndex("phaseId");

                    b.ToTable("PhaseProducts");
                });

            modelBuilder.Entity("recipeservice.Model.Product", b =>
                {
                    b.Property<int>("productId")
                        .ValueGeneratedOnAdd();

                    b.Property<int[]>("childrenProductsIds");

                    b.Property<bool>("enabled");

                    b.Property<int[]>("parentProductsIds");

                    b.Property<string>("productCode")
                        .HasMaxLength(50);

                    b.Property<string>("productDescription")
                        .HasMaxLength(100);

                    b.Property<string>("productGTIN")
                        .HasMaxLength(50);

                    b.Property<string>("productName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("productId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("recipeservice.Model.Recipe", b =>
                {
                    b.Property<int>("recipeId")
                        .ValueGeneratedOnAdd();

                    b.Property<int[]>("phasesId");

                    b.Property<int?>("productionOrderTypeId")
                        .IsRequired();

                    b.Property<string>("recipeCode")
                        .HasMaxLength(50);

                    b.Property<string>("recipeDescription")
                        .HasMaxLength(100);

                    b.Property<string>("recipeName")
                        .HasMaxLength(50);

                    b.Property<int?>("recipeProductphaseProductId");

                    b.HasKey("recipeId");

                    b.HasIndex("recipeProductphaseProductId");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("recipeservice.Model.RecipeType", b =>
                {
                    b.Property<int>("recipeTypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("typeDescription")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("typeScope")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("recipeTypeId");

                    b.ToTable("RecipeTypes");
                });

            modelBuilder.Entity("recipeservice.Model.AdditionalInformation", b =>
                {
                    b.HasOne("recipeservice.Model.Product")
                        .WithMany("additionalInformation")
                        .HasForeignKey("productId");
                });

            modelBuilder.Entity("recipeservice.Model.PhaseParameter", b =>
                {
                    b.HasOne("recipeservice.Model.Phase")
                        .WithMany("phaseParameters")
                        .HasForeignKey("phaseId");
                });

            modelBuilder.Entity("recipeservice.Model.PhaseProduct", b =>
                {
                    b.HasOne("recipeservice.Model.Phase")
                        .WithMany("phaseProducts")
                        .HasForeignKey("phaseId");
                });

            modelBuilder.Entity("recipeservice.Model.Recipe", b =>
                {
                    b.HasOne("recipeservice.Model.PhaseProduct", "recipeProduct")
                        .WithMany()
                        .HasForeignKey("recipeProductphaseProductId");
                });
#pragma warning restore 612, 618
        }
    }
}
